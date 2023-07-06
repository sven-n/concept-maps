namespace ConceptMaps.UI.Services;

public static class ModelTypeExtensions
{
    public static string RelationModelType => "relations";

    public static string NerModelType => "ner";

    public static string AsString(this ModelType modelType)
    {
        switch (modelType)
        {
            case ModelType.Ner:
                return NerModelType;
            case ModelType.Relation:
                return RelationModelType;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}