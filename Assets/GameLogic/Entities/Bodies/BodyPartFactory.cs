using System;
using System.Collections.Generic;

using Utilities.Misc;
using Utilities.XmlReader;

using Entities.Health;
using Entities.Materials;
using UnityEngine;

namespace Entities.Bodies
{
    public static class BodyPartFactory
    {
        #region XmlDefs

        private const string BodiesXmlPath = "Assets/Defs/bodyparts.xml";

        private const string RootField = "root";
        private const string ItemField = "item";
        private const string BaseStatsField = "BaseStats";
        private const string InclusionField = "Inclusion";
        private const string IsContainerField = "isContainer";
        private const string HpField = "hp";
        private const string SizeField = "size";
        private const string MaterialsField = "mat";
        private const string MaterialsNameField = "name";
        private const string MaterialsSizeField = "size";

        private static XmlReader _bodyPartXmlReader = new XmlReader(BodiesXmlPath);
        #endregion XmlDefs

        #region BodiesGeneration
        public const string HumanoidBodyName = "Humanoid";
        public const string TorsoName = "Torso";
        public const string HeadName = "Head";

        // ACCESSORS:
        public static Body HumanoidBody => GetBody(HumanoidBodyName);
        public static BodyPart HumanoidTorso => GetBodyPart(HumanoidBodyName, TorsoName);
        public static BodyPart HumanoidHead => GetBodyPart(HumanoidBodyName, HeadName);

        #endregion BodiesGeneration

        public static Dictionary<string, Body> AvailableBodies = new Dictionary<string, Body>();
        public static Dictionary<string, Dictionary<string, BodyPart>> AvailableBodyParts = new Dictionary<string, Dictionary<string, BodyPart>>();
        public static bool IsInitialized = false;

        public static void Initialize()
        {
            ReadBodyPartsFromXml();
        }

        public static Body GetBody(EBodyType bodyType) => GetBody(BodyTypes.BodyType2String(bodyType));
        public static Body GetBody(string bodyType)
        {
            try
            {
                var body = AvailableBodies[bodyType];
                return body.Clone();
            }
            catch (KeyNotFoundException e)
            {
                LoggerDebug.LogE($"Could not find body: {bodyType}");
                return null;
            }
        }

        public static BodyPart GetBodyPart(EBodyType bodyType, string name) => GetBodyPart(BodyTypes.BodyType2String(bodyType), name);
        public static BodyPart GetBodyPart(string bodyType, string name)
        {
            try
            {
                var bodyPart = AvailableBodyParts[bodyType][name];
                return bodyPart.Clone();
            }
            catch (KeyNotFoundException e)
            {
                LoggerDebug.LogE($"Could not find body part: {bodyType}/{name}");
                throw e;
            }
        }

        public static void ReadBodyPartsFromXml()
        {
            IsInitialized = true;

            AvailableBodyParts = new Dictionary<string, Dictionary<string, BodyPart>>();

            // STEP 1:
            // read names only
            List<string> bodyVariantsNames = _bodyPartXmlReader.GetChildren(new List<string>() { RootField, BaseStatsField });
            var bodyPartNamesPerVariant = new List<List<string>>();
            int countVariants = 0;
            foreach (var variantName in bodyVariantsNames)
            {
                List<string> bodyPartNames = _bodyPartXmlReader.GetChildren(new List<string>() { RootField, BaseStatsField, variantName });

                bodyPartNamesPerVariant.Add(new List<string>());
                foreach (var bodyPartName in bodyPartNames)
                {
                    //Console.WriteLine("Adding bodyPartName " + bodyPartName);
                    bodyPartNamesPerVariant[countVariants].Add(bodyPartName);
                }

                countVariants++;
            }

            // STEP 2:
            // setup BodyPart Dictionary
            for (int i = 0; i < bodyVariantsNames.Count; i++)
            {
                var variantName = bodyVariantsNames[i];

                // check if variant exists
                if (!AvailableBodyParts.ContainsKey(variantName))
                    AvailableBodyParts[variantName] = new Dictionary<string, BodyPart>();

                //Console.WriteLine("variantName: " + variantName);

                foreach (var bodyPartName in bodyPartNamesPerVariant[i])
                {
                    bool isContainer = _bodyPartXmlReader.GetFloat(new List<string>() { RootField, BaseStatsField, variantName, bodyPartName, IsContainerField }) == 1;
                    float hp = _bodyPartXmlReader.GetFloat(new List<string>() { RootField, BaseStatsField, variantName, bodyPartName, HpField });
                    float size = _bodyPartXmlReader.GetFloat(new List<string>() { RootField, BaseStatsField, variantName, bodyPartName, SizeField });

                    List<string> materialNames = _bodyPartXmlReader.GetStrings(
                        new List<string>() { RootField, BaseStatsField, variantName, bodyPartName, MaterialsField, ItemField, MaterialsNameField });
                    List<string> materialWeights = _bodyPartXmlReader.GetStrings(
                        new List<string>() { RootField, BaseStatsField, variantName, bodyPartName, MaterialsField, ItemField, MaterialsSizeField });

                    var multipliers = new List<Damage>();
                    var weights = new List<float>();
                    for (int j = 0; j < materialNames.Count; j++)
                    {
                        var materialName = materialNames[j];
                        float materialWeight = float.Parse(materialWeights[j]);
                        var material = EntityMaterialFactory.GetMaterial(materialName);
                        try
                        {
                            foreach (var mult in EntityMaterialFactory.GetMaterial(materialName).DamageMultipliers)
                            {
                                multipliers.Add(mult);
                                weights.Add(materialWeight);
                            }
                        }
                        catch (Exception e) { /* do nothing */ }
                    }

                    Debug.Log("Creating bodypart: " + bodyPartName);

                    Debug.Log("before simplifying:");
                    foreach (var mult in multipliers)
                        Debug.Log($"Multiplier [{mult.DamageType.ToString()}] " + mult.Amount);

                    multipliers = DamageMultipliers.Simplify(multipliers, weights);

                    Debug.Log("after simplifying:");
                    foreach (var mult in multipliers)
                        Debug.Log($"Multiplier [{mult.DamageType.ToString()}] " + mult.Amount);

                    // build bodypart
                    var hpSystem = new HpSystem((int)hp, multipliers);
                    var bodyPart = new BodyPart(hpSystem, bodyPartName, size);

                    Debug.Log("Created bodypart: " + bodyPart.NameCustom);

                    AvailableBodyParts[variantName][bodyPartName] = bodyPart;
                }
            }

            // TODO: convert this to recursive function to be able to support deeper body structures
            // STEP 3:
            // build bodies and add parts for containers
            List<string> bodyTypeNames = _bodyPartXmlReader.GetChildren(new List<string>() { RootField, InclusionField });
            foreach (var bodyTypeName in bodyTypeNames)
            {
                var eBodyType = BodyTypes.String2BodyType(bodyTypeName);
                var body = new Body(eBodyType);

                List<string> containerNames = _bodyPartXmlReader.GetChildren(new List<string>() { RootField, InclusionField, bodyTypeName });

                foreach (var bodyPartName in containerNames)
                {
                    BodyPart bodyPart = AvailableBodyParts[bodyTypeName][bodyPartName];
                    BodyNode bodyNode = body.AddBodyPart(bodyPart);

                    List<string> partsList = _bodyPartXmlReader.GetChildren(
                        new List<string>() { RootField, InclusionField, bodyTypeName, bodyPartName });

                    foreach (var partName in partsList)
                    {
                        List<string> customNames = _bodyPartXmlReader.GetStrings(
                            new List<string>() { RootField, InclusionField, bodyTypeName, bodyPartName, partName, ItemField });

                        BodyPart bp = AvailableBodyParts[bodyTypeName][partName].Clone();

                        if (customNames.Count == 0)
                            customNames.Add(bp.Name);

                        foreach (var customName in customNames)
                        {
                            bp.NameCustom = customName; // copy custom name
                            body.AddBodyPart(bp, ref bodyNode);
                        }
                    }

                    // write back to body
                    body.SetNode(bodyNode);
                }

                AvailableBodies[bodyTypeName] = body;
            }
        }
    }
}
