namespace ConceptMaps.UI.Services;

public static class ModelTypeExtensions
{
    public static string RelationModelType => "relations";

    public static string NrtModelType => "nrt";

    public static string AsString(this ModelType modelType)
    {
        switch (modelType)
        {
            case ModelType.Nrt:
                return NrtModelType;
            case ModelType.Relation:
                return RelationModelType;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}