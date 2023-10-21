
public class Category : ListManager<ItemDetail>, IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
}
