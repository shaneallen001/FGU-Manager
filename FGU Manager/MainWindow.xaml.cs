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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        //////////////////////////
        // Main Window
        /////////////////////////
        public MainWindow()
        {
            InitializeComponent();
        }

        //////////////////////////
        // TEST Button
        /////////////////////////
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            DisplayData();
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
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(selectedFilePath);

            try
            {
                // Common logic for finding the next available ID for both item and image
                int maxId = FindMaxId(xmlDoc, "/root/item");
                int maxImageId = FindMaxId(xmlDoc, "/root/image");
                if (maxId == -1 || maxImageId == -1)
                {
                    MessageBox.Show("Error: Could not find the <item> or <image> node in the XML file.");
                    return;
                }

                int newId = maxId + 1;
                string newIdString = "id-" + newId.ToString("D5");

                // Handling the item XML
                XmlElement newItem = xmlDoc.CreateElement(newIdString);
                XmlDocumentFragment fragment = xmlDoc.CreateDocumentFragment();
                fragment.InnerXml = xmlContentToInject;

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
                    XmlNode descriptionNode = fragment.SelectSingleNode(".//description");
                    if (descriptionNode != null)
                    {
                        XmlDocumentFragment linkListFragment = xmlDoc.CreateDocumentFragment();
                        linkListFragment.InnerXml = linkListContent;
                        descriptionNode.PrependChild(linkListFragment);
                    }

                    // Create and append the new image element
                    XmlElement newImageItem = xmlDoc.CreateElement(newImageIdString);
                    newImageItem.InnerXml = $@"
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
                <name type='string'>{imageName}</name>";
                    xmlDoc.SelectSingleNode("/root/image")?.AppendChild(newImageItem);
                }

                newItem.AppendChild(fragment);
                xmlDoc.SelectSingleNode("/root/item")?.AppendChild(newItem);
                xmlDoc.Save(selectedFilePath);

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
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(selectedFilePath);



            try
            {
                int maxId = FindMaxId(xmlDoc, "/root/npc");
                int maxImageId = FindMaxId(xmlDoc, "/root/image");
                if (maxId == -1 || maxImageId == -1)
                {
                    MessageBox.Show("Error: Could not find the <npc> or <image> node in the XML file.");
                    return;
                }

                int newId = maxId + 1;
                string newIdString = "id-" + newId.ToString("D5");

                // Create the new NPC element with a unique ID
                XmlElement newNpc = xmlDoc.CreateElement(newIdString);

                // Create and append the <locked> element
                XmlElement lockedElement = xmlDoc.CreateElement("locked");
                lockedElement.SetAttribute("type", "number");
                lockedElement.InnerText = "1";
                newNpc.AppendChild(lockedElement);

                // Create and append the <token> element
                if (!string.IsNullOrEmpty(npcselectedTokenPath))
                {
                    XmlElement tokenElement = xmlDoc.CreateElement("token");
                    tokenElement.SetAttribute("type", "token");
                    tokenElement.InnerText = "tokens/" + System.IO.Path.GetFileName(npcselectedTokenPath);
                    newNpc.AppendChild(tokenElement);
                }

                // Injecting the <linklist> if an image is selected
                if (!string.IsNullOrEmpty(npcselectedImagePath))
                {
                    string fileName = System.IO.Path.GetFileName(npcselectedImagePath);
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(npcselectedImagePath);
                    int newImageId = maxImageId + 1;
                    string newImageIdString = "id-" + newImageId.ToString("D5");

                    // Create the dynamic <linklist> content
                    string linkListContent = $@"
                        <linklist>
                            <link class='imagewindow' recordname='image.{newImageIdString}'>Image: {imageName}</link>
                        </linklist>";

                    XmlDocumentFragment fragment = xmlDoc.CreateDocumentFragment();
                    fragment.InnerXml = xmlContentToInject;

                    // Find or create the <text> node
                    XmlNode textNode = fragment.SelectSingleNode(".//text");
                    if (textNode == null)
                    {
                        // Create the <text> node if it doesn't exist
                        textNode = xmlDoc.CreateElement("text");
                        textNode.Attributes.Append(xmlDoc.CreateAttribute("type")).Value = "formattedtext";
                        //fragment.FirstChild.AppendChild(textNode);
                        fragment.AppendChild(textNode);
                    }

                    // Append the <linklist> to the <text> node
                    XmlDocumentFragment linkListFragment = xmlDoc.CreateDocumentFragment();
                    linkListFragment.InnerXml = linkListContent;
                    textNode.PrependChild(linkListFragment);

                    newNpc.AppendChild(fragment);

                    // Create and append the new image element
                    XmlElement newImageItem = xmlDoc.CreateElement(newImageIdString);
                    newImageItem.InnerXml = $@"
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
                        <name type='string'>{imageName}</name>";
                    xmlDoc.SelectSingleNode("/root/image")?.AppendChild(newImageItem);
                }
                else
                {
                    // Create a fragment for the NPC XML content
                    XmlDocumentFragment npcFragment = xmlDoc.CreateDocumentFragment();
                    npcFragment.InnerXml = xmlContentToInject;
                    newNpc.AppendChild(npcFragment);
                }

                // Append the new NPC element to the root NPC node
                xmlDoc.SelectSingleNode("/root/npc")?.AppendChild(newNpc);
                xmlDoc.Save(selectedFilePath);

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
        // Find Max ID Function
        /////////////////////////
        private int FindMaxId(XmlDocument xmlDoc, string xpath)
        {
            XmlNode parentNode = xmlDoc.SelectSingleNode(xpath);
            int maxId = 0;
            if (parentNode != null)
            {
                foreach (XmlNode childNode in parentNode.ChildNodes)
                {
                    if (childNode.Name.StartsWith("id-"))
                    {
                        int currentId = int.Parse(childNode.Name.Substring(3));
                        maxId = Math.Max(maxId, currentId);
                    }
                }
                return maxId;
            }
            return -1;
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

        private List<MagicItem> LoadPartyMagicItems(XElement parentElement)
        {
            List<MagicItem> magicItems = new List<MagicItem>();

            var inventoryItems = parentElement.Descendants("treasureparcelitemlist").Elements();
            foreach (var item in inventoryItems)
            {
                var propertiesElement = item.Element("properties");
                if (propertiesElement != null && propertiesElement.Value.Contains("Magic"))
                {
                    MagicItem magicItem = new MagicItem
                    {
                        CharacterName = parentElement.Element("name")?.Value,
                        Name = item.Element("name")?.Value,
                        Rarity = item.Element("rarity")?.Value,
                        IsAttuned = item.Element("attune")?.Value == "1"
                        // Other properties as needed
                    };
                    magicItems.Add(magicItem);
                }
            }

            return magicItems;
        }

        private List<MagicItem> LoadMagicItems(XElement parentElement)
        {
            List<MagicItem> magicItems = new List<MagicItem>();

            var inventoryItems = parentElement.Descendants("inventorylist").Elements();
            foreach (var item in inventoryItems)
            {
                var propertiesElement = item.Element("properties");
                if (propertiesElement != null && propertiesElement.Value.Contains("Magic"))
                {
                    MagicItem magicItem = new MagicItem
                    {
                        CharacterName = parentElement.Element("name")?.Value,
                        Name = item.Element("name")?.Value,
                        Rarity = item.Element("rarity")?.Value,
                        IsAttuned = item.Element("attune")?.Value == "1"
                        // Other properties as needed
                    };
                    magicItems.Add(magicItem);
                }
            }

            return magicItems;
        }

        private void DisplayData()
        {
            // Flatten the list of magic items from all players
            var allMagicItems = Players.SelectMany(player =>
                player.MagicItems.Select(item =>
                    new MagicItem
                    {
                        CharacterName = player.Name,
                        Name = item.Name,
                        Rarity = item.Rarity,
                        IsAttuned = item.IsAttuned
                    })).ToList();

            // Set the DataGrid's ItemsSource to the flattened list
            MagicItemsDataGrid.ItemsSource = allMagicItems;
        }

    }
}
