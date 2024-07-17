using System;

namespace Concrete;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ShowAttribute : Attribute
{
    public string name = null;

    public ShowAttribute(string name = null)
    {
        this.name = name;
    }
}