using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.GlobalIllumination;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using LightType = UnityEngine.LightType;

/// <summary>
/// 自定义渲染管线实例
/// </summary>
public partial class CustomRenderPipeline : RenderPipeline
{
#if UNITY_EDITOR

    private static Lightmapping.RequestLightsDelegate s_lightsDelegate =
        (Light[] lights, NativeArray<LightDataGI> output) =>
        {
            LightDataGI lightDataGI = new LightDataGI();
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                switch (light.type)
                {
                    case LightType.Directional:
                    {
                        DirectionalLight directionalLight = new DirectionalLight();
                        LightmapperUtils.Extract(light, ref directionalLight);
                        lightDataGI.Init(ref directionalLight);
                        break;
                    }
                    case LightType.Point:
                    {
                        PointLight pointLight = new PointLight();
                        LightmapperUtils.Extract(light, ref pointLight);
                        lightDataGI.Init(ref pointLight);
                        break;
                    }
                    case LightType.Spot:
                    {
                        SpotLight spotLight = new SpotLight();
                        LightmapperUtils.Extract(light, ref spotLight);
                        spotLight.innerConeAngle = light.innerSpotAngle * Mathf.Deg2Rad;
                        spotLight.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
                        lightDataGI.Init(ref spotLight);
                        break;
                    }
                    case LightType.Area:
                    {
                        RectangleLight rectangleLight = new RectangleLight();
                        LightmapperUtils.Extract(light, ref rectangleLight);
                        rectangleLight.mode = LightMode.Baked;
                        lightDataGI.Init(ref rectangleLight);
                        break;
                    }
                    default:
                        lightDataGI.InitNoBake(light.GetInstanceID());
                        break;
                }

                lightDataGI.falloff = FalloffType.InverseSquared;
                output[i] = lightDataGI;
            }
        };

    partial void InitializeForEditor()
    {
        Lightmapping.SetDelegate(s_lightsDelegate);
    }

    partial void DisposeForEditor()
    {
        Lightmapping.ResetDelegate();
    }
#endif
}
