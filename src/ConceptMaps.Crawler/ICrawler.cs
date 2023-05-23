﻿using Microsoft.Extensions.DependencyInjection;

namespace ConceptMaps.Crawler;

/// <summary>
/// Interface for a crawler implementation.
/// </summary>
public interface ICrawler
{
    /// <summary>
    /// Crawls the website and writes the results into the text writers.
    /// </summary>
    /// <param name="settings">The settings for the website which should be crawled.</param>
    /// <param name="contentWriter">The text writer where the content of the page is written to.</param>
    /// <param name="relationsWriter">The text writer to which the relationships between persons are written to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CrawlAsync(WebsiteSettings settings, TextWriter contentWriter, TextWriter relationsWriter, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extension methods for <see cref="ICrawler"/>.
/// </summary>
public static class CrawlerExtensions
{
    /// <summary>
    /// Crawls the website and writes the results into the text writers.
    /// </summary>
    /// <param name="crawler">The crawler.</param>
    /// <param name="settings">The settings for the website which should be crawled.</param>
    /// <param name="textFilePath">The target text file path.</param>
    /// <param name="relationshipFilePath">The target relationship file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async ValueTask CrawlAsync(this ICrawler crawler, WebsiteSettings settings, string textFilePath, string relationshipFilePath, CancellationToken cancellationToken)
    {
        await using var contentWriter = File.CreateText(textFilePath);
        await using var relationsWriter = File.CreateText(relationshipFilePath);
        await crawler.CrawlAsync(settings, contentWriter, relationsWriter, cancellationToken);
    }
}