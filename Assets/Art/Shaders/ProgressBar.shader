// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ProgressBar"
{
	Properties
	{
		_Emission_1("Emission_1", Color) = (0,0.1510413,0.6132076,1)
		_Emission_2("Emission_2", Color) = (0.4386792,0.9561171,1,1)
		_VoronoiSpeed("Voronoi Speed", Float) = 1
		_VoronoiScale("Voronoi Scale", Float) = 1
		_PannerSpeed("Panner Speed", Float) = 0
		_Density("Density", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		Blend One One
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Emission_1;
		uniform float4 _Emission_2;
		uniform float _PannerSpeed;
		uniform float _VoronoiScale;
		uniform float _VoronoiSpeed;
		uniform float _Density;


		float2 voronoihash8( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi8( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash8( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			
			 		}
			 	}
			}
			return F1;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime12 = _Time.y * _PannerSpeed;
			float mulTime16 = _Time.y * _VoronoiSpeed;
			float time8 = mulTime16;
			float2 voronoiSmoothId8 = 0;
			float2 coords8 = i.uv_texcoord * _VoronoiScale;
			float2 id8 = 0;
			float2 uv8 = 0;
			float fade8 = 0.5;
			float voroi8 = 0;
			float rest8 = 0;
			for( int it8 = 0; it8 <8; it8++ ){
			voroi8 += fade8 * voronoi8( coords8, time8, id8, uv8, 0,voronoiSmoothId8 );
			rest8 += fade8;
			coords8 *= 2;
			fade8 *= 0.5;
			}//Voronoi8
			voroi8 /= rest8;
			float4 lerpResult10 = lerp( _Emission_1 , _Emission_2 , sin( ( ( i.uv_texcoord.x + mulTime12 + voroi8 ) * _Density ) ));
			o.Emission = lerpResult10.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18918
1365;78;1360;747;1694.692;-159.0566;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;17;-1273.77,575.6725;Inherit;False;Property;_VoronoiSpeed;Voronoi Speed;5;0;Create;True;0;0;0;False;0;False;1;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1147.93,353.7806;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-1100.77,475.6725;Inherit;False;Property;_PannerSpeed;Panner Speed;7;0;Create;True;0;0;0;False;0;False;0;-0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;16;-1109.817,584.0886;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1108.77,657.6725;Inherit;False;Property;_VoronoiScale;Voronoi Scale;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;12;-934.0498,477.0774;Inherit;False;1;0;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;8;-930.5568,558.746;Inherit;False;0;0;1;0;8;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-741.1495,381.4773;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-755.7697,498.6725;Inherit;False;Property;_Density;Density;8;0;Create;True;0;0;0;False;0;False;0;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-623.7697,390.6725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;15;-500.5591,389.6429;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-626.3653,158.829;Inherit;False;Property;_Emission_2;Emission_2;2;0;Create;True;0;0;0;False;0;False;0.4386792,0.9561171,1,1;0.1050021,0,0.1333333,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-857.0654,159.729;Inherit;False;Property;_Emission_1;Emission_1;1;0;Create;True;0;0;0;False;0;False;0,0.1510413,0.6132076,1;0.04994448,0,0.1415094,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-158.0654,138.729;Inherit;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-473.0654,-20.27097;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;0,0.7137255,1,1;0.15262,0.01432895,0.4339623,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;10;-375.0654,341.729;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-151.0654,64.72903;Inherit;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ProgressBar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;False;4;1;False;-1;1;False;-1;0;1;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;17;0
WireConnection;12;0;18;0
WireConnection;8;0;7;0
WireConnection;8;1;16;0
WireConnection;8;2;21;0
WireConnection;13;0;7;1
WireConnection;13;1;12;0
WireConnection;13;2;8;0
WireConnection;19;0;13;0
WireConnection;19;1;20;0
WireConnection;15;0;19;0
WireConnection;10;0;4;0
WireConnection;10;1;5;0
WireConnection;10;2;15;0
WireConnection;0;2;10;0
ASEEND*/
//CHKSM=4AD8B3F0A99DE5CFA06648F132F7158382C8768D