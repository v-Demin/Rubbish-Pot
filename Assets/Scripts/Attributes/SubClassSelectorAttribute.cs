using UnityEngine;

public class SubclassSelectorAttribute : PropertyAttribute
{
    public string FilterMethodName { get; private set; }
    
    public SubclassSelectorAttribute() { }
    
    public SubclassSelectorAttribute(string filterMethodName)
    {
        FilterMethodName = filterMethodName;
    }
}