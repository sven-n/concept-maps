﻿using ConceptMaps.Crawler;
using ConceptMaps.UI.Spacy;

namespace ConceptMaps.UI.Components;

public static class RelationshipExtensions
{
    public static bool IsUndefined(this Relationship relationship)
    {
        return SpacyRelationLabel.Undefined.Equals(relationship.RelationshipTypeInSentence, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsChildren(this Relationship relationship)
    {
        return SpacyRelationLabel.Children.Equals(relationship.RelationshipTypeInSentence, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsSpouse(this Relationship relationship)
    {
        return SpacyRelationLabel.Spouse.Equals(relationship.RelationshipTypeInSentence, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsSiblings(this Relationship relationship)
    {
        return SpacyRelationLabel.Siblings.Equals(relationship.RelationshipTypeInSentence, StringComparison.InvariantCultureIgnoreCase);
    }
}