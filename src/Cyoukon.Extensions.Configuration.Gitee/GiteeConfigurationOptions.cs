using System;
using Cyoukon.Extensions.Configuration.Abstractions;

namespace Cyoukon.Extensions.Configuration.Gitee
{
    public class GiteeConfigurationOptions : GitRepositoryOptions
    {
        public string ApiBaseUrl { get; set; } = "https://gitee.com/api/v5";

        public override void Validate()
        {
            base.Validate();
        }
    }
}
