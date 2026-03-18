using System;
using System.Net.Http;
using Cyoukon.Extensions.Configuration.Abstractions;
using Cyoukon.Extensions.Configuration.Gitee.Services;
using Microsoft.Extensions.Configuration;

namespace Cyoukon.Extensions.Configuration.Gitee
{
    public static class ConfigurationManagerExtensions
    {
        private const string DefaultSectionName = "GiteeConfiguration";

        public static ConfigurationManager AddGiteeConfiguration(
            this ConfigurationManager configurationManager,
            Action<GiteeConfigurationOptions> configure)
        {
            return AddGiteeConfiguration(configurationManager, configure, null);
        }

        public static ConfigurationManager AddGiteeConfiguration(
            this ConfigurationManager configurationManager,
            Action<GiteeConfigurationOptions> configure,
            HttpClient? httpClient)
        {
            var options = new GiteeConfigurationOptions();
            configure(options);

            options.Validate();

            var source = new GiteeConfigurationSource(options, httpClient);
            ((IConfigurationBuilder)configurationManager).Add(source);

            return configurationManager;
        }

        public static ConfigurationManager AddGiteeConfiguration(
            this ConfigurationManager configurationManager,
            GiteeConfigurationOptions options)
        {
            return AddGiteeConfiguration(configurationManager, options, null);
        }

        public static ConfigurationManager AddGiteeConfiguration(
            this ConfigurationManager configurationManager,
            GiteeConfigurationOptions options,
            HttpClient? httpClient)
        {
            options.Validate();

            var source = new GiteeConfigurationSource(options, httpClient);
            ((IConfigurationBuilder)configurationManager).Add(source);

            return configurationManager;
        }

        public static ConfigurationManager AddGiteeConfiguration(
            this ConfigurationManager configurationManager,
            string sectionName = DefaultSectionName)
        {
            return AddGiteeConfiguration(configurationManager, sectionName, null);
        }

        public static ConfigurationManager AddGiteeConfiguration(
            this ConfigurationManager configurationManager,
            string sectionName,
            HttpClient? httpClient)
        {
            var options = BindOptionsFromConfiguration(configurationManager, sectionName);
            options.Validate();

            var source = new GiteeConfigurationSource(options, httpClient);
            ((IConfigurationBuilder)configurationManager).Add(source);

            return configurationManager;
        }

        private static GiteeConfigurationOptions BindOptionsFromConfiguration(
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

            var options = new GiteeConfigurationOptions();
            section.Bind(options);

            return options;
        }

        public static GiteeConfigurationProvider? GetGiteeConfigurationProvider(
            this IConfigurationRoot configurationRoot)
        {
            foreach (var provider in configurationRoot.Providers)
            {
                if (provider is GiteeConfigurationProvider giteeProvider)
                {
                    return giteeProvider;
                }
            }
            return null;
        }

        public static GiteeWebhookHandler? CreateWebhookHandler(
            this IConfigurationRoot configurationRoot,
            GiteeConfigurationOptions options)
        {
            var provider = configurationRoot.GetGiteeConfigurationProvider();
            if (provider == null)
            {
                return null;
            }
            return new GiteeWebhookHandler(provider, options);
        }
    }
}
