﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Composition;

using Analyzer.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using MSTest.Analyzers.Helpers;

namespace MSTest.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TestCleanupShouldBeValidFixer))]
[Shared]
public sealed class TestCleanupShouldBeValidFixer : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
        = ImmutableArray.Create(DiagnosticIds.TestCleanupShouldBeValidRuleId);

    public override FixAllProvider GetFixAllProvider()
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SyntaxNode node = root.FindNode(context.Span);
        if (node == null)
        {
            return;
        }

        FixtureMethodSignatureChanges fixesToApply = context.Diagnostics.Aggregate(FixtureMethodSignatureChanges.None, (acc, diagnostic) =>
        {
            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.NotStaticRule)
            {
                return acc | FixtureMethodSignatureChanges.RemoveStatic;
            }

            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.PublicRule)
            {
                return acc | FixtureMethodSignatureChanges.MakePublic;
            }

            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.ReturnTypeRule)
            {
                return acc | FixtureMethodSignatureChanges.FixReturnType;
            }

            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.NotAsyncVoidRule)
            {
                return acc | FixtureMethodSignatureChanges.FixAsyncVoid;
            }

            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.NoParametersRule)
            {
                return acc | FixtureMethodSignatureChanges.RemoveParameters;
            }

            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.NotGenericRule)
            {
                return acc | FixtureMethodSignatureChanges.RemoveGeneric;
            }

            if (diagnostic.Descriptor == TestCleanupShouldBeValidAnalyzer.NotAbstractRule)
            {
                return acc | FixtureMethodSignatureChanges.RemoveAbstract;
            }

            // return accumulator unchanged, either the action cannot be fixed or it will be fixed by default.
            return acc;
        });

        if (fixesToApply != FixtureMethodSignatureChanges.None)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixResources.AssemblyCleanupShouldBeValidCodeFix,
                    ct => FixtureMethodFixer.FixSignatureAsync(context.Document, root, node, fixesToApply, ct),
                    nameof(TestCleanupShouldBeValidFixer)),
                context.Diagnostics);
        }
    }
}