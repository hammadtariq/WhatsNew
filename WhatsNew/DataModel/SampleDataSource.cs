using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace WhatsNew.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : WhatsNew.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            var group1 = new SampleDataGroup("Group-1",
                    "Food",
                    "Group Subtitle: 1",
                    "Assets/food1.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "KFC",
                    "Item Subtitle: 1",
                    "Images/FoodnDrink/KFC/KFC.jpg",
                    "What A Deal at #KFC! Now enjoy 2 Zinger burgers, 1 Fries and 2 Drinks for Rs.600 only! Limited time offer, available nationwide. Isn't this #SoGood?",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "McDonald",
                    "Item Subtitle: 2",
                    "Images/FoodnDrink/M/mcdonald.gif",
                    "McDonald’s presents Mix Premium, Chicken Plus and Chicken Value Share Boxes.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Hardees",
                    "Item Subtitle: 3",
                    "Images/FoodnDrink/Hardees/hardees1.jpg",
                    "Give thanks… with the awesomeness of the Hand-Breaded Buffalo Chicken Tenders.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "BBQ Tonight",
                    "Item Subtitle: 4",
                    "Images/FoodnDrink/BBQ/bbq1.jpg",
                    "Feeling Chilly??  Spice your weekend, Experience the best BBQ u ever had.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Pizza Hut",
                    "Item Subtitle: 5",
                    "Images/FoodnDrink/Pizza/pizza.png",
                    "There is Pizza Mia for all the 'Meat Lovers'! Mmmmmmm... who's ordering Pizza Mia for dinner tonight? .",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Clothing",
                    "Group Subtitle: 2",
                    "Assets/clothe.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Levis",
                    "Item Subtitle: 1",
                    "Images/Clothes/Levis/levis1.jpg",
                    "Wheather you are on the job or hitting the town - we have got you covered",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Cross Road",
                    "Item Subtitle: 2",
                    "Images/Clothes/crossroad/crossroad.jpg",
                    "Ready to Wear Casual and Street Wear? Hurry up UPTO 50% OFF RUSH NOW !!!!",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Bonanza",
                    "Item Subtitle: 3",
                    "Images/Clothes/Bonanza/bonanza.png",
                    "Grand sale upto 50% off. Get your Collection of this year.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                  "Chen One",
                  "Item Subtitle: 4",
                  "Images/Clothes/chenone/chenone2.jpg",
                  "Winter Calling. Grab our New arrivals that will surely keep you warm",
                  ITEM_CONTENT,
                  group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-5",
                  "Diner",
                  "Item Subtitle: 5",
                  "Images/clothes/diner/diner.jpg",
                  "Welcome New Year 2014 New Year Promotion Take an Extra upto 30% Discount on Gent's & Ladies Winter Products.. Limited time offer. ",
                  ITEM_CONTENT,
                  group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Foot Wear",
                    "Group Subtitle: 3",
                    "Assets/shoe.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Bata",
                    "Item Subtitle: 1",
                    "Images/FootWear/Bata/cover.jpg",
                    "Save upto Rs 2000 hurry up....Limited Time offer. Shoes, belts, wallets - we have men's style covered at Bata! To find the nearest Bata store near you, please go to http://www.bata.com/online-shopping/.",
                    ITEM_CONTENT,
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Gucci",
                    "Item Subtitle: 2",
                    "Images/FootWear/gucci/gucci1.jpg",
                    "Item Description: Exclusive Gucci Shoes Collection In Pakistan. Best Quality Guaranteed.",
                    ITEM_CONTENT,
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "Puma",
                    "Item Subtitle: 3",
                    "Images/FootWear/puma/puma.jpg",
                    "Item Description: Your Most Powerful Kick has been hidden in your foot all along This boot? Its made to Enhance it",
                    ITEM_CONTENT,
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Nike",
                    "Item Subtitle: 4",
                    "Images/FootWear/Nike/Nike.jpg",
                    "Item Description: The 2014 BHM Collection was designed under the theme, Sport Royalty, to honor Nike’s kings and queens of sport: LeBron James, Carmelo Anthony, Allyson Felix, Maya Moore and Ishod Wair. The new royalty do not reach for the crown; they put in the work and claim it. In cities everywhere, they pursue of a new era of leadership every day. For the kings and queens of our communities, each challenge holds the opportunity to honor the past, by holding to principles more valuable than gold. The mission is clear. Answer the call. Earn the crown. ",
                    ITEM_CONTENT,
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Servis",
                    "Item Subtitle: 5",
                    "Images/Footwear/servis/servis.png",
                    "Item Description: Time to stock up for the next season! Head to your nearest Servis store and check out what we have in store for you!.",
                    ITEM_CONTENT,
                    group3));
           
            this.AllGroups.Add(group3);

            var group4 = new SampleDataGroup("Group-4",
                    "Accessories",
                    "Group Subtitle: 4",
                    "Assets/access.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group4.Items.Add(new SampleDataItem("Group-4-Item-1",
                    "RayBan",
                    "Item Subtitle: 1",
                    "Images/Accessories/rayban/rayban.jpg",
                    "Item Description: The Future is Amber, Ambermatic lenses are coming!",
                    ITEM_CONTENT,
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-2",
                    "Rado Watches",
                    "Item Subtitle: 2",
                    "Images/Accessories/rado/rado.jpg",
                    "Item Description: Rado Hyperchrome 181 DIAMONDS.",
                    ITEM_CONTENT,
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
                    "Gucci bags",
                    "Item Subtitle: 3",
                    "Images/Accessories/gucci/guccibag.jpg",
                    "Item Description: Grab new Shoulder Bags from Gucci.The best Brand of Leather bags in World.",
                    ITEM_CONTENT,
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-4",
                    "Nike Bands",
                    "Item Subtitle: 4",
                    "Images/Accessories/nike/nikeband.jpg",
                    "Item Description: Welcome to the world of NikeFuel, and the Nike+ FuelBand SE, the latest version of the company's wearable wireless fitness band.",
                    ITEM_CONTENT,
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-5",
                    "Ck Perfumes",
                    "Item Subtitle: 5",
                    "Images/Accessories/perfume/calvinklien.jpg",
                    "Item Description: endless euphoria, a new fragrance for her by Calvin Klein.",
                    ITEM_CONTENT,
                    group4));
           
            this.AllGroups.Add(group4);

          
        }
    }
}
