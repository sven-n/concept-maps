namespace ConceptMaps.UI.Data;

/// <summary>
/// The state of a prepared sentence.
/// </summary>
public enum SentenceState
{
    /// <summary>
    /// The initial state.
    /// </summary>
    Initial,

    /// <summary>
    /// The sentence is getting processed by the analyze-function.
    /// </summary>
    Processing,

    /// <summary>
    /// The sentence was processed by the analzye-function.
    /// </summary>
    Processed,

    /// <summary>
    /// The sentence was reviewed and accepted as training data by the user.
    /// </summary>
    Reviewed,

    /// <summary>
    /// The sentence was hidden by the user.
    /// </summary>
    Hidden,

    /// <summary>
    /// The sentence was deleted by the user. This state should not occur in
    /// items which are listed on the data prepare page, because deleting would
    /// remove it from the list.
    /// </summary>
    Deleted,
}