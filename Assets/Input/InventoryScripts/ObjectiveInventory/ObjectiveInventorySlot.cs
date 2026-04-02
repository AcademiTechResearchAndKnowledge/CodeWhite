[System.Serializable]
public class ObjectiveInventorySlot
{
    public ObjectiveItemData item;
    public int amount;

    public ObjectiveInventorySlot(ObjectiveItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }
}