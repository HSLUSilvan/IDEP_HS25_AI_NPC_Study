using System;

[Serializable]
public class RiddleDefinition
{
    public string id;
    public string question;
    public string acceptanceCriteria;
    public string hint;

    public override string ToString()
    {
        return $"[{id}] {question} (Criteria: {acceptanceCriteria})";
    }
}
