// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Design;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;

[assembly: AssemblyTitle("Enterprise Library Transient Fault Handling Application Block")]
[assembly: AssemblyDescription("Enterprise Library Transient Fault Handling Application Block")]

[assembly: AssemblyVersion("6.0.0.0")]
[assembly: AssemblyFileVersion("6.0.1311.0")]
[assembly: AssemblyInformationalVersion("6.0.1311-prerelease")]

[assembly: ComVisible(false)]

[assembly: SecurityTransparent]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: HandlesSection(RetryPolicyConfigurationSettings.SectionName)]
[assembly: AddApplicationBlockCommand(
            RetryPolicyConfigurationSettings.SectionName,
            typeof(RetryPolicyConfigurationSettings),
            TitleResourceName = "AddRetryPolicyConfigurationSettings",
            TitleResourceType = typeof(DesignResources),
            CommandModelTypeName = TransientFaultHandlingDesignTime.CommandTypeNames.AddTransientFaultHandlingBlockCommand)]