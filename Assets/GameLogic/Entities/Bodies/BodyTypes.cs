namespace Entities.Bodies
{
    public enum EBodyType
    {
        Humanoid,
        Tank,
        None
    }

    public static class BodyTypes
    {
        public const string BodyHumanoidName = "Humanoid";
        public const string BodyTankName = "Tank";
        public const string BodyNoneName = "None";

        public static string BodyType(EBodyType type)
        {
            switch (type)
            {
                case EBodyType.Humanoid:
                    return BodyHumanoidName;
                case EBodyType.Tank:
                    return BodyTankName;
                default:
                    return BodyNoneName;
            }
        }

        public static EBodyType BodyType(string bodyType)
        {
            switch (bodyType)
            {
                case BodyHumanoidName:
                    return EBodyType.Humanoid;
                case BodyTankName:
                    return EBodyType.Tank;
                default:
                    return EBodyType.None;
            }
        }
    }
}
