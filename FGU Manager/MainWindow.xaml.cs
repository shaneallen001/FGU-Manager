using System.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace FGU_Manager
{
    public partial class MainWindow : Window
    {

        //////////////////////////
        // Variables
        /////////////////////////
        private string selectedFilePath;
        private string selectedImagePath;
        private string npcselectedImagePath;
        private string npcselectedTokenPath;
        public List<Player> Players { get; set; }
        private HashSet<string> majorItemTypes = new HashSet<string> { "Weapon", "Armor", "Shield", "Ring", "Cloak", "Rod", "Wondrous Item" };

        //////////////////////////
        // Main Window
        /////////////////////////
        public MainWindow()
        {
            InitializeComponent();
        }

        //////////////////////////
        // Select File Button
        /////////////////////////
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                txtSelectedFilePath.Content = openFileDialog.FileName;
            }

            LoadData();
            DisplayData(CommonItemsDataGrid, "Common", CommonItemsText, false);
            DisplayData(UncommonItemsDataGrid, "Uncommon", UncommonItemsText, false);
            DisplayData(RareItemsDataGrid, "Rare", RareItemsText, false);
            DisplayData(VeryRareItemsDataGrid, "Very Rare", VeryRareItemsText, false);
            DisplayData(LegendaryItemsDataGrid, "Legendary", LegendaryItemsText, false);

            DisplayData(UncommonMajorItemsDataGrid, "Uncommon", UncommonMajorItemsText, true);
            DisplayData(RareMajorItemsDataGrid, "Rare", RareMajorItemsText, true);
            DisplayData(VeryRareMajorItemsDataGrid, "Very Rare", VeryRareMajorItemsText, true);
            DisplayData(LegendaryMajorItemsDataGrid, "Legendary", LegendaryMajorItemsText, true);
        }

        //////////////////////////
        // Item Tab
        // Select Image Button
        /////////////////////////
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;

                // Update the Image control to display the selected image
                BitmapImage bitmap = new BitmapImage(new Uri(selectedImagePath, UriKind.Absolute));
                DisplayImage.Source = bitmap;
            }
        }

        //////////////////////////
        // Item
        // Inject Button
        /////////////////////////
        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Please select an XML file first.");
                return;
            }

            string xmlContentToInject = PasteBox.Text;
            XDocument xmlDoc = XDocument.Load(selectedFilePath);

            try
            {
                int maxId = FindMaxId(xmlDoc, "item");
                int maxImageId = FindMaxId(xmlDoc, "image");
                if (maxId == -1 || maxImageId == -1)
                {
                    MessageBox.Show("Error: Could not find the <item> or <image> node in the XML file.");
                    return;
                }

                int newId = maxId + 1;
                string newIdString = "id-" + newId.ToString("D5");

                XElement newItem = new XElement(newIdString);
                newItem.Add(XElement.Parse(xmlContentToInject));

                // Injecting the <linklist> if an image is selected
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    string fileName = System.IO.Path.GetFileName(selectedImagePath);
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(selectedImagePath);
                    int newImageId = maxImageId + 1;
                    string newImageIdString = "id-" + newImageId.ToString("D5");

                    // Create the dynamic <linklist> content
                    string linkListContent = $@"
                    <linklist>
                    <link class='imagewindow' recordname='image.{newImageIdString}'>Image: {imageName}</link>
                    </linklist>";

                    // Insert the <linklist> into the <description> of the item
                    var descriptionNode = xmlDoc.Descendants("description").FirstOrDefault();
                    if (descriptionNode != null)
                    {
                        XElement linkListElement = XElement.Parse(linkListContent);
                        descriptionNode.AddFirst(linkListElement);
                    }

                    // Create and append the new image element
                    XElement newImageItem = XElement.Parse($@"
                    <{newImageIdString}>
                        <image type='image'>
                            <allowplayerdrawing>on</allowplayerdrawing>
                            <layers>
                                <layer>
                                    <name>{fileName}</name>
                                    <id>0</id>
                                    <parentid>-1</parentid>
                                    <type>image</type>
                                    <bitmap>campaign/images/{fileName}</bitmap>
                                </layer>
                            </layers>
                        </image>
                        <locked type='number'>1</locked>
                        <name type='string'>{imageName}</name>
                    </{newImageIdString}>");

                    var imageRoot = xmlDoc.Root.Element("image");
                    if (imageRoot != null)
                    {
                        imageRoot.Add(newImageItem);
                    }

                    xmlDoc.Root.Element("item")?.Add(newItem);
                    xmlDoc.Save(selectedFilePath);
                }
                
                MessageBox.Show("XML content and image injected successfully with ID " + newIdString);
            }
            catch (XmlException ex)
            {
                MessageBox.Show("XML Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        //////////////////////////
        // NPC
        // Select Image Button
        /////////////////////////
        private void NpcSelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                npcselectedImagePath = openFileDialog.FileName;

                // Update the Image control to display the selected image
                BitmapImage bitmap = new BitmapImage(new Uri(npcselectedImagePath, UriKind.Absolute));
                npcDisplayImage.Source = bitmap;
            }
        }

        //////////////////////////
        // NPC
        // Select Token Button
        /////////////////////////
        private void NpcSelectTokenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                npcselectedTokenPath = openFileDialog.FileName;

                // Update the Image control to display the selected image
                BitmapImage bitmap = new BitmapImage(new Uri(npcselectedTokenPath, UriKind.Absolute));
                npcDisplayToken.Source = bitmap;
            }
        }

        //////////////////////////
        // NPC
        // Inject Button
        /////////////////////////
        private void NpcInjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Please select an XML file first.");
                return;
            }

            string xmlContentToInject = npcPasteBox.Text;
            XDocument xmlDoc = XDocument.Load(selectedFilePath);

            try
            {
                int maxId = FindMaxId(xmlDoc, "npc");
                int maxImageId = FindMaxId(xmlDoc, "image");
                if (maxId == -1 || maxImageId == -1)
                {
                    MessageBox.Show("Error: Could not find the <npc> or <image> node in the XML file.");
                    return;
                }

                int newId = maxId + 1;
                string newIdString = "id-" + newId.ToString("D5");

                XElement newNpc = new XElement(newIdString);
                XElement npcFragment = XElement.Parse(xmlContentToInject);
                newNpc.Add(npcFragment);

                if (!string.IsNullOrEmpty(npcselectedImagePath))
                {
                    string fileName = System.IO.Path.GetFileName(npcselectedImagePath);
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(npcselectedImagePath);
                    int newImageId = maxImageId + 1;
                    string newImageIdString = "id-" + newImageId.ToString("D5");

                    string linkListContent = $@"
                        <linklist>
                            <link class='imagewindow' recordname='image.{newImageIdString}'>Image: {imageName}</link>
                        </linklist>";

                    XElement textNode = npcFragment.Element("text");
                    if (textNode == null)
                    {
                        textNode = new XElement("text", new XAttribute("type", "formattedtext"));
                        npcFragment.Add(textNode);
                    }

                    textNode.AddFirst(XElement.Parse(linkListContent));

                    XElement newImageItem = new XElement(newImageIdString,
                        new XElement("image", new XAttribute("type", "image"),
                            new XElement("allowplayerdrawing", "on"),
                            new XElement("layers",
                                new XElement("layer",
                                    new XElement("name", fileName),
                                    new XElement("id", "0"),
                                    new XElement("parentid", "-1"),
                                    new XElement("type", "image"),
                                    new XElement("bitmap", $"campaign/images/{fileName}")
                                )
                            )
                        ),
                        new XElement("locked", new XAttribute("type", "number"), "1"),
                        new XElement("name", new XAttribute("type", "string"), imageName)
                    );

                    xmlDoc.Root.Element("image")?.Add(newImageItem);
                }

                if (!string.IsNullOrEmpty(npcselectedTokenPath))
                {
                    XElement tokenElement = new XElement("token", new XAttribute("type", "token"),
                        "tokens/" + System.IO.Path.GetFileName(npcselectedTokenPath));
                    newNpc.Add(tokenElement);
                }

                xmlDoc.Root.Element("npc")?.Add(newNpc);
                xmlDoc.Save(selectedFilePath);

                MessageBox.Show("NPC content and image injected successfully with ID " + newIdString);
            }
            catch (XmlException ex)
            {
                MessageBox.Show("XML Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        //////////////////////////
        // Find Max ID Function
        /////////////////////////
        private int FindMaxId(XDocument xmlDoc, string elementName)
        {
            var maxId = xmlDoc.Descendants(elementName)
                              .Select(e => e.Name.LocalName.StartsWith("id-") ? int.Parse(e.Name.LocalName.Substring(3)) : 0)
                              .DefaultIfEmpty(0)
                              .Max();
            return maxId;
        }

        private void LoadData()
        {
            XDocument doc = XDocument.Load(selectedFilePath);
            Players = new List<Player>();

            // Load individual characters' items
            var charSheets = doc.Descendants("charsheet");
            foreach (var charSheet in charSheets)
            {
                foreach (var charElem in charSheet.Elements())
                {
                    Player player = new Player();

                    var nameElement = charElem.Element("name");
                    if (nameElement != null && nameElement.Attribute("type")?.Value == "string")
                    {
                        player.Name = nameElement.Value;
                    }
                    else
                    {
                        continue;
                    }

                    player.MagicItems = LoadMagicItems(charElem);
                    Players.Add(player);
                }
            }

            // Load party shared items
            var partySheet = doc.Descendants("partysheet").FirstOrDefault();
            if (partySheet != null)
            {
                Player partyPlayer = new Player { Name = "Party Sheet" };
                partyPlayer.MagicItems = LoadPartyMagicItems(partySheet);
                Players.Add(partyPlayer);
            }
        }

        // For loading items from the party sheet
        private List<MagicItem> LoadPartyMagicItems(XElement parentElement)
        {
            List<MagicItem> magicItems = new List<MagicItem>();

            var inventoryItems = parentElement.Descendants("treasureparcelitemlist").Elements();
            foreach (var item in inventoryItems)
            {
                var propertiesElement = item.Element("properties");
                if (propertiesElement != null && propertiesElement.Value.IndexOf("Magic", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    MagicItem magicItem = new MagicItem
                    {
                        CharacterName = parentElement.Element("name")?.Value,
                        Name = item.Element("name")?.Value,
                        Rarity = item.Element("rarity")?.Value,
                        //IsAttuned = item.Element("attune")?.Value == "1" ? 1 : 0
                        // Other properties as needed
                    };
                    magicItems.Add(magicItem);
                }
            }

            return magicItems;
        }

        // For loading items held by specific characters
        private List<MagicItem> LoadMagicItems(XElement parentElement)
        {
            List<MagicItem> magicItems = new List<MagicItem>();

            var inventoryItems = parentElement.Descendants("inventorylist").Elements();
            foreach (var item in inventoryItems)
            {
                var propertiesElement = item.Element("properties");
                if (propertiesElement != null && propertiesElement.Value.IndexOf("Magic", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    MagicItem magicItem = new MagicItem
                    {
                        CharacterName = parentElement.Element("name")?.Value,
                        Name = item.Element("name")?.Value,
                        Rarity = item.Element("rarity")?.Value,
                        Type = item.Element("type")?.Value,
                        //IsAttuned = item.Element("attune")?.Value == "1" ? 1 : 0
                        // Other properties as needed
                    };
                    magicItems.Add(magicItem);
                }
            }

            return magicItems;
        }

        private void DisplayData(DataGrid dataGrid, string rarity, TextBox itemCountDisplay, bool showMajorItems)
        {
            var filteredItems = Players.SelectMany(player =>
                player.MagicItems.Where(item =>
                    item.Rarity.IndexOf(rarity, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (showMajorItems == majorItemTypes.Contains(item.Type)))
                    .Select(item =>
                        new MagicItem
                        {
                            CharacterName = player.Name,
                            Name = item.Name,
                            Rarity = item.Rarity,
                            Type = item.Type,
                            //IsAttuned = item.IsAttuned
                            // Other properties as needed
                        })).ToList();

            dataGrid.ItemsSource = filteredItems;
            itemCountDisplay.Text = $"{filteredItems.Count}";
        }

    }
}