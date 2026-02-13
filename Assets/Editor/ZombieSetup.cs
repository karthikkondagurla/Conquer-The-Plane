using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class ZombieSetup : MonoBehaviour
{
    [InitializeOnLoadMethod]
    static void SetupZombie()
    {
        string enemiesPath = "Assets/Characters/Enemies/Scary Zombie Pack";
        string prefabPath = "Assets/Resources/ZombieEnemy.prefab";


        if (File.Exists(prefabPath))
        {
             AssetDatabase.DeleteAsset(prefabPath);
        }
        
        if (!Directory.Exists(enemiesPath))
        {
            Debug.LogWarning("Zombie Pack folder not found at " + enemiesPath);
            return;
        }

        Debug.Log("Starting Zombie Setup...");

        // Ensure Resources folder exists
        if (!Directory.Exists("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");

        // 1. Configure Import Settings (Humanoid & Loop)
        string[] files = Directory.GetFiles(enemiesPath, "*.fbx");
        foreach (string file in files)
        {
            ModelImporter importer = AssetImporter.GetAtPath(file) as ModelImporter;
            if (importer == null) continue;

            bool changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                changed = true;
            }

            // Loop settings for specific anims or all
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips.Length == 0)
            {
                clips = new ModelImporterClipAnimation[] { new ModelImporterClipAnimation() { name = "Default", takeName = importer.importedTakeInfos[0].name } };
            }
            
            // Material & Texture Setup for Character Model
            if (file.Contains("Mremireh O Desbiens")) 
            {
                string texturePath = "Assets/Characters/Enemies/Textures";
                string materialPath = "Assets/Characters/Enemies/Materials";

                if (!Directory.Exists(texturePath)) Directory.CreateDirectory(texturePath);
                if (!Directory.Exists(materialPath)) Directory.CreateDirectory(materialPath);

                // Extract Textures
                importer.ExtractTextures(texturePath);
                
                // Set to External Materials (Legacy logic often works best for Mixamo)
                importer.materialLocation = ModelImporterMaterialLocation.External;
                importer.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
            }
            else
            {
                 // For Animations, we don't really care about materials
                 importer.materialImportMode = ModelImporterMaterialImportMode.None;
            }


            
            // Force Setup
            foreach (var clip in clips)
            {
                 clip.loopTime = true; 
            }
            importer.clipAnimations = clips;
            changed = true; 

            if (changed) importer.SaveAndReimport();
        }

        // 2. Create Animator Controller
        string controllerPath = "Assets/Characters/Enemies/ZombieController.controller";
        // Create if not exists
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add Parameters
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Trigger); // Changed to Trigger for Attack

        // Add States
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Load Clips - simplistic search
        AnimationClip idleClip = LoadClip(enemiesPath, "zombie idle");
        AnimationClip runClip = LoadClip(enemiesPath, "zombie run");
        if (runClip == null) runClip = LoadClip(enemiesPath, "zombie walk");
        AnimationClip attackClip = LoadClip(enemiesPath, "zombie attack");

        var stateIdle = rootStateMachine.AddState("Idle");
        stateIdle.motion = idleClip;

        var stateRun = rootStateMachine.AddState("Run");
        stateRun.motion = runClip;

        var stateAttack = rootStateMachine.AddState("Attack");
        stateAttack.motion = attackClip;

        // Transitions
        // Idle <-> Run
        var toRun = stateIdle.AddTransition(stateRun);
        toRun.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        toRun.duration = 0.1f;
        toRun.hasExitTime = false; // IMMEDIATE transition

        var toIdle = stateRun.AddTransition(stateIdle);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        toIdle.duration = 0.1f;
        toIdle.hasExitTime = false; // IMMEDIATE transition

        // Any -> Attack
        var anyToAttack = rootStateMachine.AddAnyStateTransition(stateAttack);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "IsAttacking");
        anyToAttack.duration = 0.1f;
        anyToAttack.hasExitTime = false;

        // Attack -> Idle (Exit)
        var fromAttack = stateAttack.AddTransition(stateIdle);
        fromAttack.hasExitTime = true; 
        fromAttack.exitTime = 0.9f;
        fromAttack.duration = 0.1f;


        // 3. Create Prefab
        // Load the character FBX
        GameObject characterModel = AssetDatabase.LoadAssetAtPath<GameObject>(enemiesPath + "/Mremireh O Desbiens.fbx");
        if (characterModel == null)
        {
             Debug.LogError("Could not find character model Mremireh O Desbiens.fbx");
             return;
        }

        GameObject instance = Instantiate(characterModel);
        instance.name = "ZombieEnemy";

        // Add Components
        CapsuleCollider collider = instance.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0, 1, 0);
        collider.height = 1.8f;
        collider.radius = 0.45f; // Increased to prevent clipping
        
        Rigidbody rb = instance.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false; // Allow physics to resolve collisions 

        instance.AddComponent<EnemyAI>();

        Animator anim = instance.GetComponent<Animator>();
        if (anim == null) anim = instance.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;
        anim.applyRootMotion = false; 

        // CRITICAL FIX: Assign the Avatar from the imported model
        Animator modelAnimator = characterModel.GetComponent<Animator>();
        if (modelAnimator != null)
        {
            anim.avatar = modelAnimator.avatar;
        }

        // Save Prefab (Force overwrite)
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);

        Debug.Log("Zombie Setup Complete! specific prefab created at " + prefabPath);
    }

    static AnimationClip LoadClip(string folder, string partialName)
    {
        string[] files = Directory.GetFiles(folder, "*.fbx");
        foreach (string f in files)
        {
            if (f.ToLower().Contains(partialName.ToLower()))
            {
               // Load the FBX, iterate objects to find animation clip
               Object[] assets = AssetDatabase.LoadAllAssetsAtPath(f);
               foreach (Object obj in assets)
               {
                   if (obj is AnimationClip clip && !obj.name.Contains("__preview__"))
                   {
                       return clip;
                   }
               }
            }
        }
        return null;
    }
}
