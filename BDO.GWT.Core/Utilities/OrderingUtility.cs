
using BDO.GWT.Core.Constants;

namespace BDO.GWT.Core.Utilities;

public static class OrderingUtility
{
    public static IEnumerable<Person> OrderPeopleByPeopleSort(IEnumerable<Person> people, List<Guid>? peopleSort)
    {
        if (peopleSort == null || peopleSort.Count == 0)
        {
            return people;
        }

        var orderedPeople = new List<Person>();

        var peopleByNodeGuid = people.ToDictionary(x => x.SystemFields.ContentItemGUID, x => x);

        foreach (var personNodeGuid in peopleSort)
        {
            if (!peopleByNodeGuid.TryGetValue(personNodeGuid, out var person))
            {
                continue;
            }

            orderedPeople.Add(person);
        }

        return orderedPeople;
    }

    public static IEnumerable<Person> OrderPeopleByCriteria(IEnumerable<Person> people, string? orderCriteria, bool isDescendingOrder)
    {
        if (orderCriteria is null)
        {
            return people;
        }

        Func<Person, string?> orderBy = orderCriteria switch
        {
            PeopleOrderCriteria.FirstName => x => x.PersonFirstName,
            PeopleOrderCriteria.LastName => x => x.PersonLastName,
            PeopleOrderCriteria.JobTitle => x => x.PersonJobTitle,
            _ => throw new NotSupportedException($"Ordering by {orderCriteria} is not supported."),
        };

        return (isDescendingOrder ? people.OrderByDescending(orderBy) : people.OrderBy(orderBy)).ToArray();
    }   
}
