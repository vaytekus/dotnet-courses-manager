using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Courses.App.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected const int _searchDebounceMs = 300;

    protected static string BuildRequiredError(IEnumerable<string> fields)
    {
        var sb = new StringBuilder();
        var list = new List<string>(fields);

        for (int i = 0; i < list.Count; i++)
        {
            if (i == 0)
            {
                sb.Append(list[i]);
            }
            else if (i == list.Count - 1)
            {
                sb.Append($" and {list[i]}");
            }
            else
            {
                sb.Append($", {list[i]}");
            }
        }

        return sb + (list.Count > 1 ? " are required!" : " is required!");
    }
}
