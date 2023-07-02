namespace ConceptMaps.UI.Components;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// For color codes, see also: https://talyian.github.io/ansicolors/
/// </remarks>
public partial class ConsoleText
{
    private static readonly string ResetColors = "[0m";

    private static readonly string Bold = "[1m";

    private static readonly string Underline = "[4m";

    private static readonly string ForegroundId = "38";

    private static readonly string BackgroundId = "48";

    private static readonly Regex BasicColorsRegex = new Regex(@"^\[\d+m");

    private static readonly Regex Colors256Regex = new Regex(@"^\[(38|48);5;(\d+)m");

    private static readonly Regex ColorsRgbRegex = new Regex(@"^\[(38|48);2;(?<r>\d+);(?<g>\d+);(?<b>\d+)m");

    private static readonly Regex RestOfLineRegex = new Regex(@"^\[[\d;]+m(?<text>.*)$");

    private static readonly Color[] BasicColors = new[]
    {
        Color.FromArgb(0, 0, 0),
        Color.FromArgb(142, 0, 0),
        Color.FromArgb(0, 142, 0),
        Color.FromArgb(142, 142, 0),
        Color.FromArgb(0, 0, 142),
        Color.FromArgb(142, 0, 142),
        Color.FromArgb(0, 142, 142),
        Color.FromArgb(142, 142, 142),
        Color.FromArgb(51, 51, 51),
        Color.FromArgb(214, 51, 51),
        Color.FromArgb(51, 214, 51),
        Color.FromArgb(214, 214, 51),
        Color.FromArgb(51, 51, 214),
        Color.FromArgb(214, 51, 214),
        Color.FromArgb(51, 214, 214),
        Color.FromArgb(214, 214, 214),
    };

    private static readonly Dictionary<string, Color> BasicForegroundColors = new()
    {
        { "[30m", BasicColors[0] },
        { "[31m", BasicColors[1] },
        { "[32m", BasicColors[2] },
        { "[33m", BasicColors[3] },
        { "[34m", BasicColors[4] },
        { "[35m", BasicColors[5] },
        { "[36m", BasicColors[6] },
        { "[37m", BasicColors[7] },
    };

    private static readonly Dictionary<string, Color> BasicBackgroundColors = new()
    {
        { "[40m", BasicColors[0] },
        { "[41m", BasicColors[1] },
        { "[42m", BasicColors[2] },
        { "[43m", BasicColors[3] },
        { "[44m", BasicColors[4] },
        { "[45m", BasicColors[5] },
        { "[46m", BasicColors[6] },
        { "[47m", BasicColors[7] },
    };

    /// <summary>
    /// Gets or sets the console text.
    /// </summary>
    /// <value>
    /// The console text.
    /// </value>
    [Parameter]
    [Required]
    public string Text { get; set; } = string.Empty;

    private List<TextSpan> Spans { get; } = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        string? currentBackColor = null;
        string? currentForeColor = null;
        bool isUnderlined = false;
        bool isBold = false;
        var lines = this.Text.Split(Environment.NewLine);
        this.Spans.Clear();
        foreach (var line in lines)
        {
            var lineSpans = line.Split("\u001b", StringSplitOptions.RemoveEmptyEntries); // Split by ESC char

            if (lineSpans.Length == 0)
            {
                continue;
            }

            var lineContentAdded = false;
            this.Spans.Add(new("<nobr>"));
            foreach (var lineSpan in lineSpans)
            {
                this.ParseTextAttributes(lineSpan, ref currentBackColor, ref currentForeColor, ref isBold, ref isUnderlined);
                if (RestOfLineRegex.Match(lineSpan) is { Success: true } rest)
                {
                    if (!string.IsNullOrEmpty(rest.Groups[1].Value))
                    {
                        this.Spans.Add(new TextSpan(rest.Groups[1].Value, currentForeColor, currentBackColor, isBold, isUnderlined));
                        lineContentAdded = true;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(lineSpan))
                {
                    this.Spans.Add(new TextSpan(lineSpan, currentForeColor, currentBackColor, isBold, isUnderlined));
                    lineContentAdded = true;
                }
            }
            this.Spans.Add(new("</nobr>"));
            if (lineContentAdded)
            {
                this.Spans.Add(new("<br>"));
            }
        }
    }

    private void ParseTextAttributes(
        string lineSpan, ref string? backColor, ref string? foreColor, ref bool isBold, ref bool isUnderlined)
    {
        if (lineSpan.StartsWith(ResetColors))
        {
            backColor = null;
            foreColor = null;
            isBold = false;
            isUnderlined = false;
            return;
        }

        if (lineSpan.StartsWith(Bold))
        {
            isBold = true;
            return;
        }

        if (lineSpan.StartsWith(Underline))
        {
            isUnderlined = true;
            return;
        }

        if (BasicColorsRegex.Match(lineSpan) is { Success: true } basicColorMatch)
        {
            if (BasicForegroundColors.TryGetValue(basicColorMatch.Value, out var fcolor))
            {
                foreColor = fcolor.Name;
            }

            if (BasicBackgroundColors.TryGetValue(basicColorMatch.Value, out var bcolor))
            {
                backColor = bcolor.Name;
            }

            return;
        }

        if (Colors256Regex.Match(lineSpan) is { Success: true } colors256Match)
        {
            var colorNr = int.Parse(colors256Match.Groups[2].Value);
            var color = GetByAnsi256Color(colorNr);
            if (colors256Match.Groups[1].Value == ForegroundId)
            {
                foreColor = ColorTranslator.ToHtml(color);
            }
            else
            {
                backColor = ColorTranslator.ToHtml(color);
            }

            return;
        }

        if (ColorsRgbRegex.Match(lineSpan) is { Success: true } rgbMatch)
        {
            var color = Color.FromArgb(
                int.Parse(rgbMatch.Groups["r"].Value),
                int.Parse(rgbMatch.Groups["g"].Value),
                int.Parse(rgbMatch.Groups["b"].Value));
            if (rgbMatch.Groups[1].Value == ForegroundId)
            {
                foreColor = ColorTranslator.ToHtml(color);
            }
            else
            {
                backColor = ColorTranslator.ToHtml(color);
            }
        }
    }

    private Color GetByAnsi256Color(int colorNr)
    {
        if (colorNr >= 232)
        {
            var c = (colorNr - 232) * 10 + 8;
            return Color.FromArgb(c, c, c);
        }

        if (colorNr < 16)
        {
            return BasicColors[colorNr];
        }
        
        colorNr -= 16;

        var remainder = colorNr % 36;
        var r = (colorNr / 36) / (5 * 255);
        var g = (remainder / 6) / (5 * 255);
        var b = (remainder % 6) / 5 * 255;

        return Color.FromArgb(r, g, b);
    }

    private record TextSpan(string Text, string? ForegroundColor = null, string? BackgroundColor = null, bool IsBold = false, bool IsUnderlined = false)
    {
        public string CssStyle
        {
            get
            {
                var result = "";
                if (this.ForegroundColor is not null)
                {
                    result = $"color: {this.ForegroundColor};";
                }

                if (this.BackgroundColor is not null)
                {
                    result += $"background-color: {this.BackgroundColor};";
                }

                if (this.IsBold)
                {
                    result += "font-weight: bold;";
                }

                if (this.IsUnderlined)
                {
                    result += "text-decoration-line: underline;";
                }

                return result;
            }
        }
    }
}
