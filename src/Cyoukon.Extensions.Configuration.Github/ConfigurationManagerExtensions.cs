using System;
using System.Net.Http;
using Cyoukon.Extensions.Configuration.Github.Services;
using Microsoft.Extensions.Configuration;

namespace Cyoukon.Extensions.Configuration.Github
{
    public static class ConfigurationManagerExtensions
    {
        private const string DefaultSectionName = "GithubConfiguration";

        public static ConfigurationManager AddGithubConfiguration(
            this ConfigurationManager configurationManager,
            Action<GithubConfigurationOptions> configure)
        {
            return AddGithubConfiguration(configurationManager, configure, null);
        }

        public static ConfigurationManager AddGithubConfiguration(
            this ConfigurationManager configurationManager,
            Action<GithubConfigurationOptions> configure,
            HttpClient? httpClient)
        {
            var options = new GithubConfigurationOptions();
            configure(options);

            options.Validate();

            var source = new GithubConfigurationSource(options, httpClient);
            ((IConfigurationBuilder)configurationManager).Add(source);

            return configurationManager;
        }

        public static ConfigurationManager AddGithubConfiguration(
            this ConfigurationManager configurationManager,
            GithubConfigurationOptions options)
        {
            return AddGithubConfiguration(configurationManager, options, null);
        }

        public static ConfigurationManager AddGithubConfiguration(
            this ConfigurationManager configurationManager,
            GithubConfigurationOptions options,
            HttpClient? httpClient)
        {
            options.Validate();

            var source = new GithubConfigurationSource(options, httpClient);
            ((IConfigurationBuilder)configurationManager).Add(source);

            return configurationManager;
        }

        public static ConfigurationManager AddGithubConfiguration(
            this ConfigurationManager configurationManager,
            string sectionName = DefaultSectionName)
        {
            return AddGithubConfiguration(configurationManager, sectionName, null);
        }

        public static ConfigurationManager AddGithubConfiguration(
            this ConfigurationManager configurationManager,
            string sectionName,
            HttpClient? httpClient)
        {
            var options = BindOptionsFromConfiguration(configurationManager, sectionName);
            options.Validate();

            var source = new GithubConfigurationSource(options, httpClient);
            ((IConfigurationBuilder)configurationManager).Add(source);

            return configurationManager;
        }

        private static GithubConfigurationOptions BindOptionsFromConfiguration(
            ConfigurationManager configurationManager,
            string sectionName)
        {
            var section = configurationManager.GetSection(sectionName);
            if (!section.Exists())
            {
                throw new InvalidOperationException(
                    $"Configuration section '{sectionName}' not found. " +
                    $"Please ensure the section exists in your configuration file.");
            }

            var options = new GithubConfigurationOptions();
            section.Bind(options);

            return options;
        }

        public static GithubConfigurationProvider? GetGithubConfigurationProvider(
            this IConfigurationRoot configurationRoot)
        {
            foreach (var provider in configurationRoot.Providers)
            {
                if (provider is GithubConfigurationProvider githubProvider)
                {
                    return githubProvider;
                }
            }
            return null;
        }

        public static GithubWebhookHandler? CreateWebhookHandler(
            this IConfigurationRoot configurationRoot,
            GithubConfigurationOptions options)
        {
            var provider = configurationRoot.GetGithubConfigurationProvider();
            if (provider == null)
            {
                return null;
            }
            return new GithubWebhookHandler(provider, options);
        }
    }
}
