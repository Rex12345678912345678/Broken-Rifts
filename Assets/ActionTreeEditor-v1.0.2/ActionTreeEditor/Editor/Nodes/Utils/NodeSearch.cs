using System;
using System.Collections.Generic;
using System.Linq;

namespace ActionTreeEditor.Nodes
{
    public enum SearchFilter
    {
        ObjectName,
        SaveAs,
        SearchRoot,
        Duration,
        SoundName,
        Tag,
        NodeType
    }
    
    public static class NodeSearch
    {
        // automatically checks for alias without '-' too
        private static readonly (string Alias, SearchFilter Filter)[] FilterAliases =
        {
            ("object-name:", SearchFilter.ObjectName),
            ("obj-name:",    SearchFilter.ObjectName),
            ("asset-name:",  SearchFilter.ObjectName),
            ("name-id:",     SearchFilter.ObjectName),
            
            ("save-as:",     SearchFilter.SaveAs),
            ("search-root:", SearchFilter.SearchRoot),
            ("duration:",    SearchFilter.Duration),
            ("sound:",       SearchFilter.SoundName),
            ("sound-name:",  SearchFilter.SoundName),
            ("tag:",         SearchFilter.Tag),
            
            ("type:",        SearchFilter.NodeType),
            ("node-type:",   SearchFilter.NodeType)
        };

        public static Dictionary<SearchFilter, List<string>> ParseSearchFilters(string searchText)
        {
            var filters = new Dictionary<SearchFilter, List<string>>();
            if (string.IsNullOrEmpty(searchText))
                return filters;

            var tokens = searchText.Split(new [] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawToken in tokens)
            {
                var token = rawToken.Trim();

                foreach (var item in FilterAliases)
                {
                    var alias = item.Alias;
                    var filter = item.Filter;
                    
                    if (!token.StartsWith(alias, StringComparison.InvariantCultureIgnoreCase))
                    {
                        alias = alias.Replace("-", string.Empty);
                        
                        if (!token.StartsWith(alias, StringComparison.InvariantCultureIgnoreCase))
                            continue;
                    }

                    var rawValue = token.Substring(alias.Length).Trim();
                    
                    var values = rawValue.Split('|')
                        .Select(v => v.Trim())
                        .Where(v => v.Length > 0)
                        .ToList();

                    if (!filters.TryGetValue(filter, out var existing))
                    {
                        filters[filter] = values;
                    }
                    else
                    {
                        existing.AddRange(values);
                    }
                    break;
                }
            }

            return filters;
        }
        
        public static NodeType? SearchEnum(string input)
        {
            var parts = input
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0);

            foreach (var part in parts)
            {
                var type = SearchEnumPart(part);
                if (type != null)
                    return type;
            }

            return null;
        }
        
        private static NodeType? SearchEnumPart(string input)
        {
            if (string.IsNullOrEmpty(input)) 
                return null;
    
            input = input.ToLowerInvariant();
            
            if (input == "InstantiateProp".ToLowerInvariant())
                input = nameof(NodeType.InstatiateProp).ToLowerInvariant();
    
            var values = Enum.GetValues(typeof(NodeType))
                .Cast<NodeType?>()
                .ToList();
    
            // exact match
            var exact = values.FirstOrDefault(v => v.ToString().ToLowerInvariant() == input);
            if (exact != null) 
                return exact;
    
            // starts with
            var matches = values
                .Where(v => v.ToString().ToLowerInvariant().StartsWith(input))
                .ToList();
            
            if (matches.Count == 1) 
                return matches[0];
    
            return null;
        }

        public static IEnumerable<float> ParseFloats(IEnumerable<string> values)
        {
            foreach (var val in values)
            {
                if (float.TryParse(val, out var f))
                    yield return f;
            }
        }
    }
}