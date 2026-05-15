Shader "UI/UISegmentor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _InnerRadius ("Inner Radius (0-1)", Range(0, 1)) = 0.5
        _OuterRadius ("Outer Radius (0-1)", Range(0, 1)) = 1.0
        _AngleOffset ("Rotation Offset (0-360)", Range(0, 360)) = 0
        _FillAmount ("Segment Width (0-360)", Range(0, 360)) = 90
        _Smoothness ("Edge Smoothing", Range(0.001, 0.1)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _InnerRadius;
            float _OuterRadius;
            float _AngleOffset;
            float _FillAmount;
            float _Smoothness;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

fixed4 frag(v2f IN) : SV_Target
{
    // Расчет UV и радиуса (без изменений)
    float2 uv = IN.texcoord * 2.0 - 1.0;
    float dist = length(uv);
    
    // Получаем угол 0-360, где 0 — это верх (12 часов)
    float angle = degrees(atan2(uv.x, uv.y)); // Используем x,y для поворота системы координат
    float finalAngle = fmod(angle + 360.0, 360.0);
    
    // Сдвиг и нормализация угла сегмента
    float relativeAngle = fmod(finalAngle - _AngleOffset + 360.0, 360.0);

    // Параметры сглаживания
    // Переводим радиальное сглаживание в угловое (примерно)
    float sEdge = _Smoothness * 60.0; 

    // --- Магия маски сегмента ---
    
    // 1. Сглаживание начала (у 0 градусов)
    float startMask = smoothstep(0, sEdge, relativeAngle);
    
    // 2. Сглаживание конца (у _FillAmount)
    float endMask = smoothstep(_FillAmount, _FillAmount - sEdge, relativeAngle);
    
    // 3. Убираем "дырку":
    // Когда _FillAmount приближается к 360, нам нужно, чтобы начало (0°) перестало быть прозрачным.
    // Мы плавно смешиваем startMask с единицей, когда круг почти замкнут.
    float closeGapFactor = saturate((_FillAmount - (360.0 - sEdge)) / sEdge);
    float finalStartMask = lerp(startMask, 1.0, closeGapFactor);

    float segmentMask = finalStartMask * endMask;

    // Специальная проверка для полного круга (360)
    if (_FillAmount >= 360.0) segmentMask = 1.0;

    // --- Маска кольца ---
    float circleMask = smoothstep(_InnerRadius - _Smoothness, _InnerRadius, dist) * 
                       smoothstep(_OuterRadius + _Smoothness, _OuterRadius, dist);

    // Итоговый цвет
    fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
    col.a *= circleMask * segmentMask;

    clip(col.a - 0.001);
    return col;
}
            ENDCG
        }
    }
}