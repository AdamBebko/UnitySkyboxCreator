using System.IO;
using UnityEditor;
using UnityEngine;

namespace AdamsTools.SkyboxCreator
{
    public class CreateSkyboxFromCamera : ScriptableWizard
    {
        const int NumCubeSides = 6;
        const float CubeMapFieldOfView = 90f;
        
        static readonly int SkyboxShaderTexProperty = Shader.PropertyToID("_Tex");

        [SerializeField] string _skyboxName = "NewSkybox";
        [SerializeField] Camera _cameraToBake;
        [SerializeField] int _resolution = 1024;
        [SerializeField] int _antiAliasing = 4;

        string TextureFileName => $"{_skyboxName}_texture.png";
        string TextureFilePath => Path.Combine("Assets/", TextureFileName);
    
        string MaterialFileName => $"{_skyboxName}_skybox.mat";
        string MaterialFilePath => Path.Combine("Assets/", MaterialFileName);

        /// <summary>
        /// Order matters. See https://docs.unity3d.com/Manual/class-Cubemap.html
        /// </summary>
        readonly Vector3[] _photoDirections = 
        {
            new Vector3(0, 90, 0),  // right   +x axis
            new Vector3(0, -90, 0), // left    -x axis
            new Vector3(-90, 0, 0), // up      +y axis
            new Vector3(90, 0, 0),  // down    -y axis
            new Vector3(0, 0, 0),   // forward +z axis
            new Vector3(0, 180, 0), // back    -z axis
        };

        [MenuItem("Adam's Tools/Generate Skybox from Camera")]
        static void CreateWizard()
        {
            CreateSkyboxFromCamera wizard = DisplayWizard<CreateSkyboxFromCamera>("Generate Cube Map", "Generate");
            
            wizard._cameraToBake = Camera.main; //defaults to main cam
            if (wizard._cameraToBake == null)
            {
                wizard._cameraToBake = FindAnyObjectByType<Camera>(); // if no main cam, finds one.
            }
        }

        void OnWizardCreate()
        {
            // Store initial state
            Vector3 initialCameraPosition = _cameraToBake.transform.position;
            Quaternion initialCameraRotation = _cameraToBake.transform.rotation;
            RenderTexture initialRenderTexture = _cameraToBake.targetTexture;
            float initialFieldOfView = _cameraToBake.fieldOfView;
            int initialAntiAliasing = QualitySettings.antiAliasing;
            
            // Setup
            _cameraToBake.fieldOfView = CubeMapFieldOfView;
            QualitySettings.antiAliasing = _antiAliasing;
        
            // Render
            Texture2D compositeCubeImage = RenderCubeImages();
            
            // Cleanup
            _cameraToBake.transform.position = initialCameraPosition;
            _cameraToBake.transform.rotation = initialCameraRotation;
            _cameraToBake.fieldOfView = initialFieldOfView;
            QualitySettings.antiAliasing = initialAntiAliasing;
        
            SaveCubeImage(compositeCubeImage);
            CreateSkyboxMaterial();
        }

        void OnWizardUpdate()
        {
            helpString = "This Wizard will generate a Cube Map from camera taken at its position. It ignores orientation";
        }

        void CreateSkyboxMaterial()
        {
            Texture savedTex = AssetDatabase.LoadAssetAtPath<Texture>(TextureFilePath);
            Material skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));
            skyboxMaterial.SetTexture(SkyboxShaderTexProperty, savedTex);
            AssetDatabase.CreateAsset(skyboxMaterial, MaterialFilePath);
        
            AssetDatabase.Refresh();
        }

        void SaveCubeImage(Texture2D compositeCubeImage)
        {
            byte[] bytes = compositeCubeImage.EncodeToPNG(); 
            File.WriteAllBytes(TextureFilePath, bytes);
            AssetDatabase.ImportAsset(TextureFilePath, ImportAssetOptions.ForceUpdate);
        
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(TextureFilePath);
            importer.textureShape = TextureImporterShape.TextureCube;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        
            AssetDatabase.Refresh();
        }

        Texture2D RenderCubeImages()
        {
            Texture2D cubeImage = new Texture2D(_resolution*NumCubeSides, _resolution, TextureFormat.RGBA32, false);
            RenderTexture renderTexture = new RenderTexture(_resolution, _resolution, 24);
            
            RenderTexture initialRenderTexture = _cameraToBake.targetTexture;
            _cameraToBake.targetTexture = renderTexture;
            
            for (int i = 0; i < NumCubeSides; i++)
            {
                _cameraToBake.transform.eulerAngles = _photoDirections[i];
                _cameraToBake.Render();
                
                RenderTexture.active = renderTexture;
                cubeImage.ReadPixels(new Rect(0, 0, _resolution, _resolution), i*_resolution, 0);
                RenderTexture.active = null;
            }
            renderTexture.Release();
            _cameraToBake.targetTexture = initialRenderTexture;
            return cubeImage;
        }
    }
}
