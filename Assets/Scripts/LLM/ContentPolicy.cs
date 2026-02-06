using System;
using System.Text.RegularExpressions;

public sealed class ContentPolicy
{

    // Self-harm / suicide
    private static readonly Regex SelfHarm =
        new Regex(@"\b(kill\s*yourself|kys|suicide|self[-\s]?harm|cut(ting)?\s*(myself|yourself)|end\s*my\s*life)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Hate / slurs
    private static readonly Regex HateHarassment =
        new Regex(@"\b(nazi|kkk|genocide|gas\s*(the|all)|racial\s*slur|kill\s*(all|those)\b)|\b(faggot|nigger|kike|spic|chink|wetback)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Graphic violence / gore
    private static readonly Regex GraphicViolence =
        new Regex(@"\b(dismember|decapitat(e|ion)|gore|blood\s*(spray|spurting)|rip\s*out\s*organs|torture|snuff)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Drugs
    private static readonly Regex DrugsHard =
        new Regex(@"\b(how\s*to\s*make\s*(meth|lsd)|cook\s*meth|drug\s*deal|sell\s*drugs)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool IsUserInputAllowed(string text, out string reason)
    {
        reason = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            reason = "Empty message.";
            return false;
        }

        if (SelfHarm.IsMatch(text))
        {
            reason = "I can’t help with self-harm content.";
            return false;
        }

        if (HateHarassment.IsMatch(text))
        {
            reason = "Hate/harassment content isn’t allowed.";
            return false;
        }

        if (GraphicViolence.IsMatch(text))
        {
            reason = "Graphic violence isn’t allowed in this game.";
            return false;
        }

        if (DrugsHard.IsMatch(text))
        {
            reason = "I can’t help with drug-making or dealing content.";
            return false;
        }

        return true;
    }

    public bool IsModelOutputAllowed(string text, out string reason)
    {
        reason = null;

        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (SelfHarm.IsMatch(text))
        {
            reason = "NPC output violated safety rules (self-harm).";
            return false;
        }

        if (HateHarassment.IsMatch(text))
        {
            reason = "NPC output violated safety rules (hate/harassment).";
            return false;
        }

        if (GraphicViolence.IsMatch(text))
        {
            reason = "NPC output violated safety rules (graphic violence).";
            return false;
        }

        if (DrugsHard.IsMatch(text))
        {
            reason = "NPC output violated safety rules (drug content).";
            return false;
        }

        return true;
    }

    public static string GetPg13RefusalLine()
    {
        return "The door’s runes dim. “No. Keep it PG-13, traveler. Back to the riddle.”";
    }
}
