﻿using ColossalFramework;
using ICities;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Rainfall
{
    class StormDistributionIO
    {
        private StreamReader reader;
        private StreamWriter writer;
        private Dictionary<string, SortedList<float, float>> depthCurves;
        private Dictionary<string, SortedList<float, float>> intensityCurves;
        private string fileDirectory;
        private string filePath;
        private string fileName;
        private Dictionary<string, List<float>> defaultDepthCurves;
        public static StormDistributionIO instance;
        
        public StormDistributionIO()
        {
            instance = this;
            fileDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\";
            filePath = fileDirectory + "RFDepthCurves.csv";
            fileName = "RFDepthCurves.csv";
            if (!File.Exists(fileName))
            {
                initializeDefaultDepthCurves();
                try
                {
                    string deliminator = ",";
                    writer = new StreamWriter(File.OpenWrite(fileName));
                    StringBuilder firstLine = new StringBuilder();
                    foreach (KeyValuePair<string, List<float>> column in defaultDepthCurves)
                    {
                        firstLine.Append(column.Key + deliminator);
                    }
                    firstLine.Remove(firstLine.Length - 1, 1);
                    writer.WriteLine(firstLine);

                    for (int i = 0; i < defaultDepthCurves["Time"].Count; i++)
                    {
                        StringBuilder nextLine = new StringBuilder();
                        foreach (KeyValuePair<string, List<float>> column in defaultDepthCurves)
                        {
                            nextLine.Append(column.Value[i].ToString() + deliminator);
                        }
                        nextLine.Remove(nextLine.Length - 1, 1);
                        writer.WriteLine(nextLine);
                    }
                    writer.Close();
                    Debug.Log("[RF].StormDistributionIO Successfullly wrote new intensity curves file.");
                }
                catch (IOException ex)
                {
                    Debug.Log("[RF].StormDistributionIO Could not write Depth Curve file encountered excpetion " + ex.ToString());
                }

            }
            
            try
            {
                reader = new StreamReader(File.OpenRead(fileName));
                depthCurves = new Dictionary<string, SortedList<float, float>>();
                string firstLine = reader.ReadLine();
                string[] firstLineNames = firstLine.Split(',');
                for (int i = 1; i < firstLineNames.Length; i++)
                {
                    if (firstLineNames[i] != "")
                    {
                        depthCurves.Add(firstLineNames[i], new SortedList<float, float>());
                       
                    }
                }
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    for (int i = 1; i < firstLineNames.Length; i++)
                    {
                        if (firstLineNames[i] != "")
                        {
                            depthCurves[firstLineNames[i]].Add((float)(Convert.ToDecimal(values[0])), (float)Convert.ToDecimal(values[i]));
                          
                        }
                    }
                }
                reader.Close();

                Debug.Log("[RF].StormDistributionIO Successfullly imported depth curves file.");
                
                List<string> failedDepthCurves = new List<string>();
                Dictionary<string, SortedList<float, float>> reducedDepthCurves = new Dictionary<string, SortedList<float, float>>();
                intensityCurves = new Dictionary<string, SortedList<float, float>>();
                foreach(KeyValuePair<string, SortedList<float, float>> pairs in depthCurves)
                {
                   if (!reviewDepthCurve(pairs.Value, ModSettings._maxStormDuration))
                    {
                        failedDepthCurves.Add(pairs.Key);
                    } else
                    {
                        SortedList<float, float> newReducedCurve = new SortedList<float, float>();
                        reduceDuration(30f, pairs.Value, ref newReducedCurve);
                        reducedDepthCurves.Add(pairs.Key, newReducedCurve);
                        intensityCurves.Add(pairs.Key, GetIntensityCurve(pairs.Value));
                    }
                }
                foreach (string depthCurveName in failedDepthCurves)
                {
                    depthCurves.Remove(depthCurveName);
                }
                
                /*
                //Used to create default data
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("private Dictionary<string, List<float>> defaultDepthCurves = new Dictionary<string, List<float>>");
                sb.AppendLine("{");

                for (int i = 1; i < firstLineNames.Length; i++)
                {
                    if (firstLineNames[i] != "")
                    {
                        sb.AppendLine("{");
                        if (i == 0)
                            sb.Append("\"" + firstLineNames[i] + "\", new List<float> {");
                        else
                            sb.Append("\"" + firstLineNames[i] + "\", new List<float> {");
                        foreach (KeyValuePair<float, float> pair in reducedDepthCurves[firstLineNames[i]])
                        {
                            if (i == 0)
                                sb.Append(pair.Key + ", ");
                            else
                                sb.Append(pair.Value + "f, ");
                        }
                        sb.AppendLine("}");
                        sb.AppendLine("},");
                    }

                     

                }
                sb.AppendLine("};");
                Debug.Log(sb); 
                
                 */
            }
            catch (FileNotFoundException)
            {
                Debug.Log("[RF].StormDistributionIO file not found at " + fileDirectory + fileName);
            }
            catch (DirectoryNotFoundException)
            {
                Debug.Log("[RF].StormDistributionIO Directory not found at " + fileDirectory);
            }
            catch (IOException ex)
            {
                Debug.Log("[RF].StormDistributionIO Could not read Intensity Curve file encountered excpetion " + ex.ToString());
            }
        }
        private void initializeDefaultDepthCurves()
        {
            defaultDepthCurves = new Dictionary<string, List<float>>
            {

                {"Time", new List<float> {0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 66, 69, 72, 75, 78, 81, 84, 87, 90, 93, 96, 99, 102, 105, 108, 111, 114, 117, 120, 123, 126, 129, 132, 135, 138, 141, 144, 147, 150, 153, 156, 159, 162, 165, 168, 171, 174, 177, 180, 183, 186, 189, 192, 195, 198, 201, 204, 207, 210, 213, 216, 219, 222, 225, 228, 231, 234, 237, 240, 243, 246, 249, 252, 255, 258, 261, 264, 267, 270, 273, 276, 279, 282, 285, 288, 291, 294, 297, 300, 303, 306, 309, 312, 315, 318, 321, 324, 327, 330, 333, 336, 339, 342, 345, 348, 351, 354, 357, 360, 363, 366, 369, 372, 375, 378, 381, 384, 387, 390, 393, 396, 399, 402, 405, 408, 411, 414, 417, 420, 423, 426, 429, 432, 435, 438, 441, 444, 447, 450, 453, 456, 459, 462, 465, 468, 471, 474, 477, 480, 483, 486, 489, 492, 495, 498, 501, 504, 507, 510, 513, 516, 519, 522, 525, 528, 531, 534, 537, 540, 543, 546, 549, 552, 555, 558, 561, 564, 567, 570, 573, 576, 579, 582, 585, 588, 591, 594, 597, 600, 603, 606, 609, 612, 615, 618, 621, 624, 627, 630, 633, 636, 639, 642, 645, 648, 651, 654, 657, 660, 663, 666, 669, 672, 675, 678, 681, 684, 687, 690, 693, 696, 699, 702, 705, 708, 711, 714, 717, 720, 723, 726, 729, 732, 735, 738, 741, 744, 747, 750, 753, 756, 759, 762, 765, 768, 771, 774, 777, 780, 783, 786, 789, 792, 795, 798, 801, 804, 807, 810, 813, 816, 819, 822, 825, 828, 831, 834, 837, 840, 843, 846, 849, 852, 855, 858, 861, 864, 867, 870, 873, 876, 879, 882, 885, 888, 891, 894, 897, 900, 903, 906, 909, 912, 915, 918, 921, 924, 927, 930, 933, 936, 939, 942, 945, 948, 951, 954, 957, 960, 963, 966, 969, 972, 975, 978, 981, 984, 987, 990, 993, 996, 999, 1002, 1005, 1008, 1011, 1014, 1017, 1020, 1023, 1026, 1029, 1032, 1035, 1038, 1041, 1044, 1047, 1050, 1053, 1056, 1059, 1062, 1065, 1068, 1071, 1074, 1077, 1080, 1083, 1086, 1089, 1092, 1095, 1098, 1101, 1104, 1107, 1110, 1113, 1116, 1119, 1122, 1125, 1128, 1131, 1134, 1137, 1140, 1143, 1146, 1149, 1152, 1155, 1158, 1161, 1164, 1167, 1170, 1173, 1176, 1179, 1182, 1185, 1188, 1191, 1194, 1197, 1200, 1203, 1206, 1209, 1212, 1215, 1218, 1221, 1224, 1227, 1230, 1233, 1236, 1239, 1242, 1245, 1248, 1251, 1254, 1257, 1260, 1263, 1266, 1269, 1272, 1275, 1278, 1281, 1284, 1287, 1290, 1293, 1296, 1299, 1302, 1305, 1308, 1311, 1314, 1317, 1320, 1323, 1326, 1329, 1332, 1335, 1338, 1341, 1344, 1347, 1350, 1353, 1356, 1359, 1362, 1365, 1368, 1371, 1374, 1377, 1380, 1383, 1386, 1389, 1392, 1395, 1398, 1401, 1404, 1407, 1410, 1413, 1416, 1419, 1422, 1425, 1428, 1431, 1434, 1437, 1440}},

                {"Type I - Pacific Southwest/AK/HI", new List<float> {0f, 0.00087f, 0.00174f, 0.002611f, 0.003482f, 0.004353f, 0.005224f, 0.006096f, 0.006968f, 0.00784f, 0.008712f, 0.009585f, 0.010458f, 0.011331f, 0.012204f, 0.013078f, 0.013952f, 0.014826f, 0.0157f, 0.016575f, 0.01745f, 0.018325f, 0.0192f, 0.020076f, 0.020952f, 0.021828f, 0.022704f, 0.023581f, 0.024458f, 0.025335f, 0.026212f, 0.02709f, 0.027968f, 0.028846f, 0.029724f, 0.030603f, 0.031482f, 0.032361f, 0.03324f, 0.03412f, 0.035f, 0.035884f, 0.036775f, 0.037673f, 0.038578f, 0.039491f, 0.040411f, 0.041338f, 0.042272f, 0.043214f, 0.044163f, 0.045119f, 0.046082f, 0.047053f, 0.04803f, 0.049016f, 0.050008f, 0.051008f, 0.052014f, 0.053029f, 0.05405f, 0.055079f, 0.056114f, 0.057158f, 0.058208f, 0.059266f, 0.06033f, 0.061403f, 0.062482f, 0.063569f, 0.064662f, 0.065764f, 0.066872f, 0.067988f, 0.06911f, 0.070241f, 0.071378f, 0.072523f, 0.073675f, 0.074834f, 0.076f, 0.077171f, 0.078345f, 0.079522f, 0.080702f, 0.081884f, 0.08307f, 0.084257f, 0.085448f, 0.086641f, 0.087837f, 0.089036f, 0.090238f, 0.091442f, 0.092649f, 0.093859f, 0.095072f, 0.096287f, 0.097505f, 0.098726f, 0.09995f, 0.101176f, 0.102405f, 0.103637f, 0.104872f, 0.106109f, 0.10735f, 0.108592f, 0.109838f, 0.111086f, 0.112337f, 0.113591f, 0.114848f, 0.116107f, 0.11737f, 0.118634f, 0.119902f, 0.121172f, 0.122445f, 0.123721f, 0.125f, 0.126294f, 0.127614f, 0.128962f, 0.130336f, 0.131738f, 0.133166f, 0.134622f, 0.136104f, 0.137614f, 0.13915f, 0.140714f, 0.142304f, 0.143922f, 0.145566f, 0.147238f, 0.148936f, 0.150662f, 0.152414f, 0.154194f, 0.156f, 0.157824f, 0.159656f, 0.161496f, 0.163344f, 0.1652f, 0.167064f, 0.168936f, 0.170816f, 0.172704f, 0.1746f, 0.176504f, 0.178416f, 0.180336f, 0.182264f, 0.1842f, 0.186144f, 0.188096f, 0.190056f, 0.192024f, 0.194f, 0.196032f, 0.198168f, 0.200408f, 0.202752f, 0.2052f, 0.207752f, 0.210408f, 0.213168f, 0.216032f, 0.219f, 0.222068f, 0.225232f, 0.228492f, 0.231848f, 0.2353f, 0.238848f, 0.242492f, 0.246232f, 0.250068f, 0.254f, 0.258072f, 0.262328f, 0.266768f, 0.271392f, 0.2762f, 0.281192f, 0.286368f, 0.291728f, 0.297272f, 0.303f, 0.310016f, 0.319424f, 0.331224f, 0.345416f, 0.362f, 0.387843f, 0.424314f, 0.463162f, 0.496141f, 0.515f, 0.523825f, 0.5322f, 0.540125f, 0.5476f, 0.554625f, 0.5612f, 0.567325f, 0.573f, 0.578225f, 0.583f, 0.587495f, 0.59188f, 0.596155f, 0.60032f, 0.604375f, 0.60832f, 0.612155f, 0.61588f, 0.619495f, 0.623f, 0.62643f, 0.62982f, 0.63317f, 0.63648f, 0.63975f, 0.64298f, 0.64617f, 0.64932f, 0.65243f, 0.6555f, 0.65853f, 0.66152f, 0.66447f, 0.66738f, 0.67025f, 0.67308f, 0.67587f, 0.67862f, 0.68133f, 0.684f, 0.686637f, 0.68925f, 0.691837f, 0.6944f, 0.696937f, 0.69945f, 0.701937f, 0.7044f, 0.706837f, 0.70925f, 0.711637f, 0.714f, 0.716337f, 0.71865f, 0.720937f, 0.7232f, 0.725437f, 0.72765f, 0.729837f, 0.732f, 0.734137f, 0.73625f, 0.738337f, 0.7404f, 0.742437f, 0.74445f, 0.746437f, 0.7484f, 0.750337f, 0.75225f, 0.754137f, 0.756f, 0.757837f, 0.75965f, 0.761437f, 0.7632f, 0.764937f, 0.76665f, 0.768337f, 0.77f, 0.771647f, 0.77329f, 0.774927f, 0.77656f, 0.778188f, 0.77981f, 0.781427f, 0.78304f, 0.784647f, 0.78625f, 0.787847f, 0.78944f, 0.791027f, 0.79261f, 0.794187f, 0.79576f, 0.797327f, 0.79889f, 0.800447f, 0.802f, 0.803547f, 0.80509f, 0.806627f, 0.80816f, 0.809688f, 0.81121f, 0.812727f, 0.81424f, 0.815747f, 0.81725f, 0.818747f, 0.82024f, 0.821727f, 0.82321f, 0.824688f, 0.82616f, 0.827627f, 0.82909f, 0.830547f, 0.832f, 0.833448f, 0.83489f, 0.836328f, 0.83776f, 0.839187f, 0.84061f, 0.842027f, 0.84344f, 0.844847f, 0.84625f, 0.847647f, 0.84904f, 0.850428f, 0.85181f, 0.853187f, 0.85456f, 0.855927f, 0.85729f, 0.858648f, 0.86f, 0.861347f, 0.86269f, 0.864027f, 0.86536f, 0.866687f, 0.86801f, 0.869327f, 0.87064f, 0.871947f, 0.87325f, 0.874547f, 0.87584f, 0.877127f, 0.87841f, 0.879687f, 0.88096f, 0.882227f, 0.88349f, 0.884748f, 0.886f, 0.887247f, 0.88849f, 0.889728f, 0.89096f, 0.892188f, 0.89341f, 0.894628f, 0.89584f, 0.897047f, 0.89825f, 0.899447f, 0.90064f, 0.901828f, 0.90301f, 0.904187f, 0.90536f, 0.906527f, 0.90769f, 0.908847f, 0.91f, 0.911147f, 0.91229f, 0.913427f, 0.91456f, 0.915687f, 0.91681f, 0.917928f, 0.91904f, 0.920148f, 0.92125f, 0.922347f, 0.92344f, 0.924527f, 0.92561f, 0.926687f, 0.92776f, 0.928827f, 0.92989f, 0.930947f, 0.932f, 0.933047f, 0.93409f, 0.935128f, 0.93616f, 0.937187f, 0.93821f, 0.939227f, 0.94024f, 0.941248f, 0.94225f, 0.943248f, 0.94424f, 0.945227f, 0.94621f, 0.947187f, 0.94816f, 0.949128f, 0.95009f, 0.951047f, 0.952f, 0.952947f, 0.95389f, 0.954827f, 0.95576f, 0.956687f, 0.95761f, 0.958527f, 0.95944f, 0.960347f, 0.96125f, 0.962147f, 0.96304f, 0.963927f, 0.96481f, 0.965687f, 0.96656f, 0.967427f, 0.96829f, 0.969147f, 0.97f, 0.970847f, 0.97169f, 0.972527f, 0.97336f, 0.974187f, 0.97501f, 0.975827f, 0.97664f, 0.977447f, 0.97825f, 0.979047f, 0.97984f, 0.980627f, 0.98141f, 0.982187f, 0.98296f, 0.983727f, 0.98449f, 0.985247f, 0.986f, 0.986748f, 0.98749f, 0.988227f, 0.98896f, 0.989687f, 0.99041f, 0.991127f, 0.99184f, 0.992547f, 0.99325f, 0.993947f, 0.99464f, 0.995327f, 0.99601f, 0.996687f, 0.99736f, 0.998027f, 0.99869f, 0.999347f, 1f }},

                {"Type IA - Pacific Northwest", new List<float> {0f, 0.001142f, 0.00224f, 0.003297f, 0.004319f, 0.005312f, 0.006279f, 0.007226f, 0.008159f, 0.009081f, 0.009998f, 0.010916f, 0.011838f, 0.01277f, 0.013718f, 0.014685f, 0.015678f, 0.0167f, 0.017757f, 0.018855f, 0.019997f, 0.02135f, 0.022753f, 0.024195f, 0.025673f, 0.027181f, 0.028714f, 0.030267f, 0.031835f, 0.033413f, 0.034996f, 0.036485f, 0.037969f, 0.039456f, 0.040946f, 0.042439f, 0.043938f, 0.045443f, 0.046954f, 0.048474f, 0.050002f, 0.051568f, 0.053148f, 0.054737f, 0.056332f, 0.057934f, 0.05954f, 0.061151f, 0.062764f, 0.06438f, 0.065997f, 0.0676f, 0.0692f, 0.0708f, 0.0724f, 0.074f, 0.0756f, 0.0772f, 0.0788f, 0.0804f, 0.082f, 0.083567f, 0.085136f, 0.086709f, 0.088289f, 0.089876f, 0.091474f, 0.093083f, 0.094706f, 0.096346f, 0.098003f, 0.099728f, 0.101473f, 0.103236f, 0.105017f, 0.106813f, 0.108625f, 0.11045f, 0.112289f, 0.114139f, 0.116001f, 0.117836f, 0.119685f, 0.121547f, 0.123421f, 0.12531f, 0.127213f, 0.129133f, 0.131069f, 0.133024f, 0.134997f, 0.136991f, 0.139005f, 0.141042f, 0.143102f, 0.145185f, 0.147294f, 0.149428f, 0.15159f, 0.153779f, 0.155997f, 0.158284f, 0.160594f, 0.162933f, 0.165298f, 0.16769f, 0.170106f, 0.172547f, 0.17501f, 0.177496f, 0.180002f, 0.182459f, 0.184942f, 0.187452f, 0.18999f, 0.192561f, 0.195166f, 0.19781f, 0.200494f, 0.203223f, 0.205998f, 0.208954f, 0.211957f, 0.214999f, 0.218077f, 0.221184f, 0.224317f, 0.227469f, 0.230637f, 0.233814f, 0.236997f, 0.239919f, 0.242849f, 0.2458f, 0.248785f, 0.251814f, 0.254897f, 0.258047f, 0.261273f, 0.264588f, 0.268001f, 0.271525f, 0.275169f, 0.278946f, 0.282865f, 0.286939f, 0.291177f, 0.295592f, 0.300193f, 0.304993f, 0.310002f, 0.320413f, 0.331419f, 0.34289f, 0.354691f, 0.366691f, 0.378755f, 0.390752f, 0.402547f, 0.414009f, 0.425003f, 0.43246f, 0.439362f, 0.445755f, 0.451682f, 0.457189f, 0.462322f, 0.467124f, 0.471641f, 0.475919f, 0.480001f, 0.484591f, 0.489038f, 0.493346f, 0.497518f, 0.501561f, 0.505478f, 0.509276f, 0.512958f, 0.51653f, 0.519998f, 0.523332f, 0.526573f, 0.529729f, 0.532805f, 0.53581f, 0.538749f, 0.541631f, 0.544462f, 0.547248f, 0.549998f, 0.552835f, 0.55564f, 0.558415f, 0.56116f, 0.563875f, 0.56656f, 0.569215f, 0.57184f, 0.574435f, 0.577f, 0.579502f, 0.581976f, 0.584425f, 0.586849f, 0.589251f, 0.591633f, 0.593997f, 0.596345f, 0.598679f, 0.601002f, 0.60339f, 0.60576f, 0.60811f, 0.61044f, 0.61275f, 0.61504f, 0.61731f, 0.61956f, 0.62179f, 0.624f, 0.62619f, 0.62836f, 0.63051f, 0.63264f, 0.63475f, 0.63684f, 0.63891f, 0.64096f, 0.64299f, 0.645f, 0.646953f, 0.648892f, 0.650815f, 0.652724f, 0.654621f, 0.656508f, 0.658387f, 0.660261f, 0.66213f, 0.663997f, 0.665864f, 0.667733f, 0.669606f, 0.671485f, 0.673372f, 0.675269f, 0.677178f, 0.679101f, 0.68104f, 0.682997f, 0.684832f, 0.686651f, 0.688463f, 0.690267f, 0.692066f, 0.693859f, 0.695649f, 0.697435f, 0.69922f, 0.701003f, 0.702874f, 0.704728f, 0.706565f, 0.708384f, 0.710187f, 0.711976f, 0.71375f, 0.715512f, 0.717261f, 0.719f, 0.720728f, 0.722448f, 0.724159f, 0.725864f, 0.727562f, 0.729255f, 0.730945f, 0.732631f, 0.734316f, 0.735999f, 0.737698f, 0.739392f, 0.741083f, 0.74277f, 0.744453f, 0.746132f, 0.747807f, 0.749478f, 0.751146f, 0.75281f, 0.75447f, 0.756126f, 0.757779f, 0.759428f, 0.761073f, 0.762714f, 0.764351f, 0.765984f, 0.767614f, 0.76924f, 0.770862f, 0.77248f, 0.774095f, 0.775706f, 0.777313f, 0.778916f, 0.780515f, 0.78211f, 0.783702f, 0.78529f, 0.786874f, 0.788454f, 0.790031f, 0.791604f, 0.793173f, 0.794738f, 0.796299f, 0.797856f, 0.79941f, 0.80096f, 0.802506f, 0.804048f, 0.805587f, 0.807122f, 0.808652f, 0.81018f, 0.811703f, 0.813222f, 0.814738f, 0.81625f, 0.817758f, 0.819262f, 0.820763f, 0.82226f, 0.823752f, 0.825242f, 0.826727f, 0.828208f, 0.829686f, 0.83116f, 0.83263f, 0.834096f, 0.835559f, 0.837018f, 0.838473f, 0.839924f, 0.841371f, 0.842814f, 0.844254f, 0.84569f, 0.847122f, 0.84855f, 0.849975f, 0.851396f, 0.852813f, 0.854226f, 0.855635f, 0.85704f, 0.858442f, 0.85984f, 0.861234f, 0.862624f, 0.864011f, 0.865394f, 0.866773f, 0.868148f, 0.869519f, 0.870886f, 0.87225f, 0.87361f, 0.874966f, 0.876318f, 0.877667f, 0.879012f, 0.880352f, 0.88169f, 0.883023f, 0.884352f, 0.885678f, 0.887f, 0.888318f, 0.889632f, 0.890943f, 0.89225f, 0.893553f, 0.894852f, 0.896147f, 0.897438f, 0.898726f, 0.90001f, 0.90129f, 0.902566f, 0.903839f, 0.905108f, 0.906373f, 0.907634f, 0.908891f, 0.910144f, 0.911394f, 0.91264f, 0.913882f, 0.91512f, 0.916355f, 0.917586f, 0.918813f, 0.920036f, 0.921255f, 0.92247f, 0.923682f, 0.92489f, 0.926094f, 0.927294f, 0.928491f, 0.929684f, 0.930872f, 0.932058f, 0.933239f, 0.934416f, 0.93559f, 0.93676f, 0.937926f, 0.939088f, 0.940247f, 0.941402f, 0.942553f, 0.9437f, 0.944843f, 0.945982f, 0.947118f, 0.94825f, 0.949378f, 0.950502f, 0.951623f, 0.95274f, 0.953852f, 0.954962f, 0.956067f, 0.957168f, 0.958266f, 0.95936f, 0.96045f, 0.961536f, 0.962619f, 0.963698f, 0.964773f, 0.965844f, 0.966911f, 0.967974f, 0.969034f, 0.97009f, 0.971142f, 0.97219f, 0.973235f, 0.974276f, 0.975313f, 0.976346f, 0.977375f, 0.9784f, 0.979422f, 0.98044f, 0.981454f, 0.982464f, 0.983471f, 0.984474f, 0.985473f, 0.986468f, 0.987459f, 0.988446f, 0.98943f, 0.99041f, 0.991386f, 0.992358f, 0.993327f, 0.994292f, 0.995253f, 0.99621f, 0.997163f, 0.998112f, 0.999058f, 1f }},

                {"Type II - Noncoastal US", new List<float> {0f, 0.000501f, 0.001005f, 0.001511f, 0.00202f, 0.002531f, 0.003045f, 0.003561f, 0.00408f, 0.004601f, 0.005125f, 0.005651f, 0.00618f, 0.006711f, 0.007245f, 0.007781f, 0.00832f, 0.008861f, 0.009405f, 0.009951f, 0.0105f, 0.011051f, 0.011605f, 0.012161f, 0.01272f, 0.013281f, 0.013845f, 0.014411f, 0.01498f, 0.015551f, 0.016125f, 0.016701f, 0.01728f, 0.017861f, 0.018445f, 0.019031f, 0.01962f, 0.020211f, 0.020805f, 0.021401f, 0.022f, 0.022601f, 0.023205f, 0.023811f, 0.02442f, 0.025031f, 0.025645f, 0.026261f, 0.02688f, 0.027501f, 0.028125f, 0.028751f, 0.02938f, 0.030011f, 0.030645f, 0.031281f, 0.03192f, 0.032561f, 0.033205f, 0.033851f, 0.0345f, 0.035151f, 0.035805f, 0.036461f, 0.03712f, 0.037781f, 0.038445f, 0.039111f, 0.03978f, 0.040451f, 0.041125f, 0.041801f, 0.04248f, 0.043161f, 0.043845f, 0.044531f, 0.04522f, 0.045911f, 0.046605f, 0.047301f, 0.048f, 0.048703f, 0.04941f, 0.050123f, 0.05084f, 0.051563f, 0.05229f, 0.053023f, 0.05376f, 0.054503f, 0.05525f, 0.056003f, 0.05676f, 0.057523f, 0.05829f, 0.059063f, 0.05984f, 0.060623f, 0.06141f, 0.062203f, 0.063f, 0.063802f, 0.06461f, 0.065422f, 0.06624f, 0.067062f, 0.06789f, 0.068723f, 0.06956f, 0.070403f, 0.07125f, 0.072102f, 0.07296f, 0.073822f, 0.07469f, 0.075563f, 0.07644f, 0.077323f, 0.07821f, 0.079103f, 0.08f, 0.080903f, 0.08181f, 0.082723f, 0.08364f, 0.084562f, 0.08549f, 0.086422f, 0.08736f, 0.088303f, 0.08925f, 0.090203f, 0.09116f, 0.092122f, 0.09309f, 0.094063f, 0.09504f, 0.096022f, 0.09701f, 0.098003f, 0.099f, 0.100003f, 0.10101f, 0.102023f, 0.10304f, 0.104063f, 0.10509f, 0.106123f, 0.10716f, 0.108203f, 0.10925f, 0.110302f, 0.11136f, 0.112423f, 0.11349f, 0.114562f, 0.11564f, 0.116723f, 0.11781f, 0.118903f, 0.12f, 0.121112f, 0.12225f, 0.123412f, 0.1246f, 0.125812f, 0.12705f, 0.128312f, 0.1296f, 0.130912f, 0.13225f, 0.133612f, 0.135f, 0.136412f, 0.13785f, 0.139313f, 0.1408f, 0.142312f, 0.14385f, 0.145413f, 0.147f, 0.1486f, 0.1502f, 0.1518f, 0.1534f, 0.155f, 0.1566f, 0.1582f, 0.1598f, 0.1614f, 0.163f, 0.16462f, 0.16628f, 0.16798f, 0.16972f, 0.1715f, 0.17332f, 0.17518f, 0.17708f, 0.17902f, 0.181f, 0.18303f, 0.18512f, 0.18727f, 0.18948f, 0.19175f, 0.19408f, 0.19647f, 0.19892f, 0.20143f, 0.204f, 0.20665f, 0.2094f, 0.21225f, 0.2152f, 0.21825f, 0.2214f, 0.22465f, 0.228f, 0.23145f, 0.235f, 0.23872f, 0.24268f, 0.24688f, 0.25132f, 0.256f, 0.26092f, 0.26608f, 0.27148f, 0.27712f, 0.283f, 0.29196f, 0.30684f, 0.32764f, 0.35436f, 0.387f, 0.430793f, 0.491093f, 0.567859f, 0.634212f, 0.663f, 0.672765f, 0.68196f, 0.690585f, 0.69864f, 0.706125f, 0.71304f, 0.719385f, 0.72516f, 0.730365f, 0.735f, 0.739285f, 0.74344f, 0.747465f, 0.75136f, 0.755125f, 0.75876f, 0.762265f, 0.76564f, 0.768885f, 0.772f, 0.775015f, 0.77796f, 0.780835f, 0.78364f, 0.786375f, 0.78904f, 0.791635f, 0.79416f, 0.796615f, 0.799f, 0.801325f, 0.8036f, 0.805825f, 0.808f, 0.810125f, 0.8122f, 0.814225f, 0.8162f, 0.818125f, 0.82f, 0.821841f, 0.823665f, 0.825471f, 0.82726f, 0.829031f, 0.830785f, 0.832521f, 0.83424f, 0.835941f, 0.837625f, 0.839291f, 0.84094f, 0.842571f, 0.844185f, 0.845781f, 0.84736f, 0.848921f, 0.850465f, 0.851991f, 0.8535f, 0.854991f, 0.856465f, 0.857921f, 0.85936f, 0.860781f, 0.862185f, 0.863571f, 0.86494f, 0.866291f, 0.867625f, 0.868941f, 0.87024f, 0.871521f, 0.872785f, 0.874031f, 0.87526f, 0.876471f, 0.877665f, 0.878841f, 0.88f, 0.881147f, 0.882288f, 0.883422f, 0.88455f, 0.885672f, 0.886787f, 0.887897f, 0.889f, 0.890097f, 0.891188f, 0.892272f, 0.89335f, 0.894422f, 0.895487f, 0.896547f, 0.8976f, 0.898647f, 0.899687f, 0.900722f, 0.90175f, 0.902772f, 0.903787f, 0.904797f, 0.9058f, 0.906797f, 0.907787f, 0.908772f, 0.90975f, 0.910722f, 0.911687f, 0.912647f, 0.9136f, 0.914547f, 0.915488f, 0.916422f, 0.91735f, 0.918272f, 0.919188f, 0.920097f, 0.921f, 0.921897f, 0.922787f, 0.923672f, 0.92455f, 0.925422f, 0.926288f, 0.927147f, 0.928f, 0.928847f, 0.929688f, 0.930522f, 0.93135f, 0.932172f, 0.932987f, 0.933797f, 0.9346f, 0.935397f, 0.936187f, 0.936972f, 0.93775f, 0.938522f, 0.939288f, 0.940047f, 0.9408f, 0.941547f, 0.942288f, 0.943022f, 0.94375f, 0.944472f, 0.945187f, 0.945897f, 0.9466f, 0.947297f, 0.947987f, 0.948672f, 0.94935f, 0.950022f, 0.950688f, 0.951347f, 0.952f, 0.952649f, 0.953297f, 0.953944f, 0.95459f, 0.955234f, 0.955877f, 0.956519f, 0.95716f, 0.957799f, 0.958437f, 0.959074f, 0.95971f, 0.960344f, 0.960977f, 0.961609f, 0.96224f, 0.962869f, 0.963498f, 0.964124f, 0.96475f, 0.965374f, 0.965997f, 0.966619f, 0.96724f, 0.967859f, 0.968477f, 0.969094f, 0.96971f, 0.970324f, 0.970938f, 0.971549f, 0.97216f, 0.972769f, 0.973378f, 0.973984f, 0.97459f, 0.975194f, 0.975797f, 0.976399f, 0.977f, 0.977599f, 0.978197f, 0.978794f, 0.97939f, 0.979984f, 0.980577f, 0.981169f, 0.98176f, 0.982349f, 0.982937f, 0.983524f, 0.98411f, 0.984694f, 0.985277f, 0.985859f, 0.98644f, 0.987019f, 0.987597f, 0.988174f, 0.98875f, 0.989324f, 0.989897f, 0.990469f, 0.99104f, 0.991609f, 0.992177f, 0.992744f, 0.99331f, 0.993874f, 0.994437f, 0.994999f, 0.99556f, 0.996119f, 0.996677f, 0.997234f, 0.99779f, 0.998344f, 0.998897f, 0.999449f, 1f }},

                {"Type III - East & Gulf Coasts", new List<float> {0f, 0.0005f, 0.001f, 0.0015f, 0.002f, 0.0025f, 0.003f, 0.0035f, 0.004f, 0.0045f, 0.005f, 0.0055f, 0.006f, 0.0065f, 0.007f, 0.0075f, 0.008f, 0.0085f, 0.009f, 0.0095f, 0.01f, 0.0105f, 0.011f, 0.0115f, 0.012f, 0.0125f, 0.013f, 0.0135f, 0.014f, 0.0145f, 0.015f, 0.0155f, 0.016f, 0.0165f, 0.017f, 0.0175f, 0.018f, 0.0185f, 0.019f, 0.0195f, 0.02f, 0.020502f, 0.021008f, 0.021517f, 0.02203f, 0.022547f, 0.023068f, 0.023592f, 0.02412f, 0.024652f, 0.025188f, 0.025727f, 0.02627f, 0.026817f, 0.027367f, 0.027922f, 0.02848f, 0.029042f, 0.029608f, 0.030177f, 0.03075f, 0.031327f, 0.031907f, 0.032492f, 0.03308f, 0.033672f, 0.034267f, 0.034867f, 0.03547f, 0.036077f, 0.036687f, 0.037302f, 0.03792f, 0.038542f, 0.039168f, 0.039797f, 0.04043f, 0.041067f, 0.041708f, 0.042352f, 0.043f, 0.043652f, 0.044307f, 0.044967f, 0.04563f, 0.046297f, 0.046968f, 0.047642f, 0.04832f, 0.049002f, 0.049688f, 0.050377f, 0.05107f, 0.051767f, 0.052468f, 0.053172f, 0.05388f, 0.054592f, 0.055308f, 0.056027f, 0.05675f, 0.057477f, 0.058208f, 0.058942f, 0.05968f, 0.060422f, 0.061167f, 0.061917f, 0.06267f, 0.063427f, 0.064187f, 0.064952f, 0.06572f, 0.066492f, 0.067268f, 0.068047f, 0.06883f, 0.069617f, 0.070407f, 0.071202f, 0.072f, 0.072806f, 0.073625f, 0.074456f, 0.0753f, 0.076156f, 0.077025f, 0.077906f, 0.0788f, 0.079706f, 0.080625f, 0.081556f, 0.0825f, 0.083456f, 0.084425f, 0.085406f, 0.0864f, 0.087406f, 0.088425f, 0.089456f, 0.0905f, 0.091556f, 0.092625f, 0.093706f, 0.0948f, 0.095906f, 0.097025f, 0.098156f, 0.0993f, 0.100456f, 0.101625f, 0.102806f, 0.104f, 0.105206f, 0.106425f, 0.107656f, 0.1089f, 0.110156f, 0.111425f, 0.112706f, 0.114f, 0.115314f, 0.116657f, 0.118029f, 0.11943f, 0.120859f, 0.122317f, 0.123804f, 0.12532f, 0.126864f, 0.128437f, 0.130039f, 0.13167f, 0.133329f, 0.135017f, 0.136734f, 0.13848f, 0.140254f, 0.142057f, 0.143889f, 0.14575f, 0.147639f, 0.149557f, 0.151504f, 0.15348f, 0.155484f, 0.157517f, 0.159579f, 0.16167f, 0.163789f, 0.165937f, 0.168114f, 0.17032f, 0.172554f, 0.174817f, 0.177109f, 0.17943f, 0.181779f, 0.184157f, 0.186564f, 0.189f, 0.19148f, 0.19402f, 0.19662f, 0.19928f, 0.202f, 0.20478f, 0.20762f, 0.21052f, 0.21348f, 0.2165f, 0.21958f, 0.22272f, 0.22592f, 0.22918f, 0.2325f, 0.23588f, 0.23932f, 0.24282f, 0.24638f, 0.25f, 0.253765f, 0.25776f, 0.261985f, 0.26644f, 0.271125f, 0.27604f, 0.281185f, 0.28656f, 0.292165f, 0.298f, 0.30505f, 0.3143f, 0.32575f, 0.3394f, 0.35525f, 0.3733f, 0.39355f, 0.416f, 0.448775f, 0.5f, 0.551225f, 0.584f, 0.60645f, 0.6267f, 0.64475f, 0.6606f, 0.67425f, 0.6857f, 0.69495f, 0.702f, 0.707835f, 0.71344f, 0.718815f, 0.72396f, 0.728875f, 0.73356f, 0.738015f, 0.74224f, 0.746235f, 0.75f, 0.75362f, 0.75718f, 0.76068f, 0.76412f, 0.7675f, 0.77082f, 0.77408f, 0.77728f, 0.78042f, 0.7835f, 0.78652f, 0.78948f, 0.79238f, 0.79522f, 0.798f, 0.80072f, 0.80338f, 0.80598f, 0.80852f, 0.811f, 0.813436f, 0.815842f, 0.818221f, 0.82057f, 0.822891f, 0.825182f, 0.827446f, 0.82968f, 0.831886f, 0.834062f, 0.836211f, 0.83833f, 0.840421f, 0.842482f, 0.844516f, 0.84652f, 0.848496f, 0.850442f, 0.852361f, 0.85425f, 0.856111f, 0.857942f, 0.859746f, 0.86152f, 0.863266f, 0.864982f, 0.866671f, 0.86833f, 0.869961f, 0.871562f, 0.873136f, 0.87468f, 0.876196f, 0.877682f, 0.879141f, 0.88057f, 0.881971f, 0.883342f, 0.884686f, 0.886f, 0.887294f, 0.888575f, 0.889844f, 0.8911f, 0.892344f, 0.893575f, 0.894794f, 0.896f, 0.897194f, 0.898375f, 0.899544f, 0.9007f, 0.901844f, 0.902975f, 0.904094f, 0.9052f, 0.906294f, 0.907375f, 0.908444f, 0.9095f, 0.910544f, 0.911575f, 0.912594f, 0.9136f, 0.914594f, 0.915575f, 0.916544f, 0.9175f, 0.918444f, 0.919375f, 0.920294f, 0.9212f, 0.922094f, 0.922975f, 0.923844f, 0.9247f, 0.925544f, 0.926375f, 0.927194f, 0.928f, 0.928798f, 0.929592f, 0.930383f, 0.93117f, 0.931953f, 0.932732f, 0.933508f, 0.93428f, 0.935048f, 0.935812f, 0.936573f, 0.93733f, 0.938083f, 0.938832f, 0.939578f, 0.94032f, 0.941058f, 0.941792f, 0.942523f, 0.94325f, 0.943973f, 0.944692f, 0.945408f, 0.94612f, 0.946828f, 0.947533f, 0.948233f, 0.94893f, 0.949623f, 0.950312f, 0.950998f, 0.95168f, 0.952358f, 0.953032f, 0.953703f, 0.95437f, 0.955033f, 0.955692f, 0.956348f, 0.957f, 0.957649f, 0.958294f, 0.958937f, 0.959577f, 0.960215f, 0.960849f, 0.961481f, 0.96211f, 0.962736f, 0.963359f, 0.96398f, 0.964597f, 0.965212f, 0.965824f, 0.966434f, 0.96704f, 0.967644f, 0.968244f, 0.968842f, 0.969437f, 0.97003f, 0.970619f, 0.971206f, 0.97179f, 0.972371f, 0.972949f, 0.973525f, 0.974097f, 0.974667f, 0.975234f, 0.975799f, 0.97636f, 0.976919f, 0.977474f, 0.978027f, 0.978577f, 0.979125f, 0.979669f, 0.980211f, 0.98075f, 0.981286f, 0.981819f, 0.98235f, 0.982877f, 0.983402f, 0.983924f, 0.984444f, 0.98496f, 0.985474f, 0.985984f, 0.986492f, 0.986997f, 0.9875f, 0.987999f, 0.988496f, 0.98899f, 0.989481f, 0.989969f, 0.990455f, 0.990937f, 0.991417f, 0.991894f, 0.992369f, 0.99284f, 0.993309f, 0.993774f, 0.994237f, 0.994697f, 0.995155f, 0.995609f, 0.996061f, 0.99651f, 0.996956f, 0.997399f, 0.99784f, 0.998277f, 0.998712f, 0.999144f, 0.999574f, 1f }}
            };

        }
        public static Dictionary<string, SortedList<float, float>> GetDepthCurves()
        {
            return StormDistributionIO.instance.depthCurves;
        }
        public static string[] GetStormDistributionNames()
        {
            Dictionary<string, SortedList<float, float>> depthCurveList = StormDistributionIO.GetDepthCurves();
            string[] depthCurveNames = new string[depthCurveList.Count];
            int i = 0;
            foreach (KeyValuePair<string, SortedList<float, float>> curve in depthCurveList)
            {
                depthCurveNames[i] = curve.Key;
                i++;
            }
            return depthCurveNames;
        }
        public static bool GetIntensityCurve(string curveName, ref SortedList<float, float> curve)
        {
            if (StormDistributionIO.instance.intensityCurves.ContainsKey(curveName))
            {
                curve = StormDistributionIO.instance.intensityCurves[curveName];
                return true;
            } 
            return false;
        }
        public static bool GetSelectedIntensityCurve(ref SortedList<float, float> curve)
        {
            return StormDistributionIO.GetIntensityCurve(ModSettings.StormDistributionName, ref curve);
        }
        public static SortedList<float, float> GetIntensityCurve(SortedList<float, float> depthCurve)
        {
            SortedList<float, float> intensityCurve = new SortedList<float, float>();
            const float minToHour = 1f / 60f;
            float lastKey = -1;
            float lastValue = -1;
            foreach (KeyValuePair<float, float> pair in depthCurve)
            {
                if (lastKey == -1)
                {
                    intensityCurve.Add(0, 0);
                }
                else
                {
                    float deltaTime = (pair.Key - lastKey);
                    float deltaDepth = (pair.Value - lastValue);
                    float currentIntensity = deltaDepth / (deltaTime * minToHour);
                    intensityCurve.Add(pair.Key, currentIntensity);
                }
                lastKey = pair.Key;
                lastValue = pair.Value;
            }
            return intensityCurve;
        }
       
        public static SortedList<float, float> ScaleDepthCurve(SortedList<float, float> originalDepthCurve, float depthScale)
        {
            SortedList<float, float> scaledDepthCurve = new SortedList<float, float>();
            foreach (KeyValuePair<float, float> pair in originalDepthCurve)
            {
                scaledDepthCurve.Add(pair.Key, pair.Value*depthScale);
            }
            return scaledDepthCurve;
        }
        public static float GetMaxValue(SortedList<float, float> curve)
        {
            float maxValue = 0;
            foreach (KeyValuePair<float, float> pair in curve)
            {
                if (pair.Value > maxValue)
                    maxValue = pair.Value;
            }
            return maxValue;
        }
        public static SortedList<float, float> GetDepthCurve(SortedList<float, float> intensityCurve)
        {
            SortedList<float, float> depthCurve = new SortedList<float, float>();
            const float minToHour = 1f / 60f;
            float lastKey = -1;
            float lastValue = -1;
            float currentDepth = 0;
            foreach (KeyValuePair<float, float> pair in intensityCurve)
            {
                if (lastKey == -1)
                {
                    depthCurve.Add(0, 0);
                }
                else
                {
                    float deltaTime = (pair.Key - lastKey);
                    float averageIntensity = (pair.Value + lastValue) * 0.5f;
                    currentDepth += averageIntensity * deltaTime * minToHour;
                    depthCurve.Add(pair.Key, currentDepth);
                }
                lastKey = pair.Key;
                lastValue = pair.Value;
            }
            return depthCurve;
        }
        public static bool GetSelectedDepthCurve(ref SortedList<float, float> curve)
        {
            return StormDistributionIO.GetDepthCurve(ModSettings.StormDistributionName, ref curve);
        }
        public static bool GetDepthCurve(string curveName, ref SortedList<float, float> depthCurve)
        {
            if (StormDistributionIO.instance.depthCurves.ContainsKey(curveName))
            {
                depthCurve = StormDistributionIO.instance.depthCurves[curveName];
                return true;
            }
            return false;
        }
        public static bool reviewDepthCurve(SortedList<float, float> depthCurve, float stormDuration)
        {
            if (!depthCurve.ContainsKey(0f) || !depthCurve.ContainsKey(stormDuration))
            {
                if (!depthCurve.ContainsKey(0f))
                    Debug.Log("[RF].StormDurationIO.ReviewDepthCurve curve does not contain key 0");
                if (!depthCurve.ContainsKey(stormDuration))
                     Debug.Log("[RF].StormDurationIO.ReviewDepthCurve curve does not contain key " + stormDuration.ToString());
                return false;
            }

            try {
                if (depthCurve[0f] == 0 && depthCurve[stormDuration] == 1.0f && GetMaxValue(depthCurve) == 1.0f)
                {
                    float lastValue=0f;
                    for (float f = 0; f < stormDuration; f += ModSettings._stormTimeStep)
                    {
                        if (!depthCurve.ContainsKey(f))
                        {
                            Debug.Log("[RF].StormDurationIO.ReviewDepthCurve Curve does not have key " + f.ToString());
                            return false;
                        }

                        if (depthCurve[f] < lastValue)
                        {
                            Debug.Log("[RF].StormDurationIO.ReviewDepthCurve depthCurve at key " + f.ToString() + " = " + depthCurve[f].ToString() + " < last value = " + lastValue.ToString()); 
                            return false;
                        }
                        lastValue = depthCurve[f];
                    }
                    return true;
                } else
                {
                    if (depthCurve[0f] != 0)
                        Debug.Log("[RF].StormDurationIO.ReviewDepthCurve depthCurve[0] = " + depthCurve[0f].ToString());
                    if (depthCurve[stormDuration] != 1.0f)
                        Debug.Log("[RF].StormDurationIO.ReviewDepthCurve depthCurve[" + stormDuration.ToString() +"] = " + depthCurve[stormDuration].ToString());
                    if (GetMaxValue(depthCurve) != 1.0f)
                        Debug.Log("[RF].StormDurationIO.ReviewDepthCurve maxvalue = " + GetMaxValue(depthCurve).ToString());
                    return false;
                }
            } catch (Exception e)
            {
                Debug.Log("Couldnot review DepthCurve e encountered exception " + e.ToString());
                return false;
            }
        }
        public static bool reduceDuration(float stormDuration, SortedList<float, float> oldDepthCurve, ref SortedList<float, float> newDepthCurve)
        {
            if (stormDuration % ModSettings._stormTimeStep > 0f)
            {
                return false;
            }
            float currentTime = stormDuration;
            newDepthCurve = new SortedList<float, float>();
            if (reviewDepthCurve(oldDepthCurve, ModSettings._maxStormDuration) && stormDuration <= ModSettings._maxStormDuration) {
                float currentDepthVariance = oldDepthCurve[currentTime] - oldDepthCurve[0];
                float maxDepthVariance = 0f;
                while (currentDepthVariance >= maxDepthVariance && currentTime <= ModSettings._maxStormDuration)
                {
                    maxDepthVariance = currentDepthVariance;
                    currentTime += ModSettings._stormTimeStep;
                    currentDepthVariance = oldDepthCurve[currentTime] - oldDepthCurve[currentTime-stormDuration];
                }
                if (currentTime <= ModSettings._maxStormDuration)
                {
                    float startTime = currentTime - ModSettings._stormTimeStep - stormDuration;
                    float startDepth = oldDepthCurve[startTime];
                    for (float f=startTime; f<=startTime+stormDuration; f += ModSettings._stormTimeStep)
                    {
                        newDepthCurve.Add(f - startTime, oldDepthCurve[f] - startDepth);
                    }
                    newDepthCurve = ScaleDepthCurve(newDepthCurve, 1f / newDepthCurve[stormDuration]);
                    newDepthCurve[stormDuration] = 1f;
                    if (reviewDepthCurve(newDepthCurve, stormDuration))
                    {
                        return true;
                    }
                }
                

            }
            return false;
        }
        public static void logCurve(SortedList<float, float> curve, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name + " = ");
            foreach (KeyValuePair<float, float> pair in curve)
            {
                    sb.Append("["+pair.Key.ToString() + "]="+ pair.Value.ToString() + "f, ");
            }
            Debug.Log(sb.ToString());
            return;
        }
}
    
}
        

