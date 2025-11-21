namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

static class XMLTools
{
    const string s_xmlDir = @"..\xml\";
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (!File.Exists(xmlFilePath)) return new();
            using FileStream file = new(xmlFilePath, FileMode.Open);
            XmlSerializer x = new(typeof(List<T>));
            return x.Deserialize(file) as List<T> ?? new();
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region SaveLoadWithXElement
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
            XElement rootElem = new(xmlFileName);
            rootElem.Save(xmlFilePath);
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region XmlConfig
    /// <summary>
    /// get the int value from the config xml file and increase it by 1
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// get the string value from the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string? str = (string?)root.Element(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return str;
    }

    /// <summary>
    /// get the nullable string value from the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <returns></returns>
    public static string? GetConfigNullableStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string? str = (string?)root.Element(elemName);
        return str;
    }

    /// <summary>
    /// get the double value from the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToDoubleNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// get the nullable double value from the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static double? GetConfigNullableDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToDoubleNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// get the TimeSpan value from the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        TimeSpan ts = TimeSpan.Parse((string?)root.Element(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}"));
        return ts;
    }

    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// set the string value in the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <param name="elemVal"></param>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal);
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// set the nullable string value in the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <param name="elemVal"></param>
    public static void SetConfigNullableStringVal(string xmlFileName, string elemName, string? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        XElement? element = root.Element(elemName);

        if (element != null && elemVal == null)
            element.Remove();

        else if (element != null && elemVal != null)
            element.SetValue(elemVal);

        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// set the double value in the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <param name="elemVal"></param>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// set the TimeSpan value in the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <param name="elemVal"></param>
    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// set the nullable double value in the config xml file
    /// </summary>
    /// <param name="xmlFileName"></param>
    /// <param name="elemName"></param>
    /// <param name="elemVal"></param>
    public static void SetConfigNullableDoubleVal(string xmlFileName, string elemName, double? elemVal)
    {
        XElement root = LoadListFromXMLElement(xmlFileName);
        XElement? element = root.Element(elemName);

        // if elemVal has value, set it; else remove the element
        if (elemVal.HasValue)
        {
            if (element != null)
                element.SetValue(elemVal.Value.ToString());

            else
                root.Add(new XElement(elemName, elemVal.Value.ToString()));
        }

        else if (element != null)
            element.Remove();

        SaveListToXMLElement(root, xmlFileName);
    }
    #endregion


    #region ExtensionFuctions
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;

    /// <summary>
    /// Retrieves the string value of the specified child element, or <see langword="null"/> if the element does not
    /// exist or has no value.
    /// </summary>
    /// <param name="element">The parent <see cref="XElement"/> to search within.</param>
    /// <param name="name">The name of the child element to retrieve the value from.</param>
    /// <returns>The string value of the specified child element, or <see langword="null"/> if the child element is not found or
    /// its value is <see langword="null"/>.</returns>
    public static string? ToStringNullable(this XElement element, string name) =>
        (string?)element.Element(name);

    /// <summary>
    /// Retrieves the string value of the specified child element, or <see langword="null"/> if the element does not
    /// exist or has no value.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool? ToBoolNullable(this XElement element, string name) =>
        bool.TryParse((string?)element.Element(name), out var result) ? (bool?)result : null;
    #endregion

}