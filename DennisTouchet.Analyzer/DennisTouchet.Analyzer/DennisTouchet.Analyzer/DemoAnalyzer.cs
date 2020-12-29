using DennisTouchet.Analyzer.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace DennisTouchet.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    class DemoAnalyzer : DiagnosticAnalyzer
    {
        // Metadata for the Analyzer
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleIdentifiers.DemoStringComparison,
            "Specify StringComparison in string.Equals",
            "StringComparison is missing",
            RuleCategories.Usage,
            DiagnosticSeverity.Error,
            true,
            "Ensure you compare strings the way that it is expected."
            );

        // Register the list of rules this DiagnosticAnalizer supports
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // The AnalyzeNode method will be called for each InvocationExpression of the Syntax tree
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // invocationExpr.Expression is the expression before "(", here "string.Equals".
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            // In this case it should be a MemberAccessExpressionSyntax, with a member name "Equals"
            var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            
            if(memberAccessExpression == null || 
                memberAccessExpression.Name.ToString() != nameof(string.Equals))
            {
                return;
            }

            // Now get the semantic model of the node to determine its type
            // (e.g. In this case it can be defined as string or System.String)
            var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

            // Check that the method is member of class string
            if(methodSymbol == null ||
                methodSymbol.ContainingType.SpecialType != SpecialType.System_String)
            {
                return;
            }

            // If there are not 3 arguments, the comparison type is missing => report it
            // We could improve this validation by checking the types of the arguments, but it would be a little longer for this post.
            var argumentList = invocationExpression.ArgumentList;

            if ((argumentList?.Arguments.Count ?? 0) == 2)
            {
                var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
