using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Thbgm {
  [TestFixture]
  public class ProductInfoTests {
    [Test]
    public void TestEmbeddedProductInfo()
    {
      Assert.IsNotNull(ProductInfo.Th06);
      Assert.IsNotNull(ProductInfo.Th07);
      Assert.IsNotNull(ProductInfo.Th08tr);
      Assert.IsNotNull(ProductInfo.Th08);
      Assert.IsNotNull(ProductInfo.Th09tr);
      Assert.IsNotNull(ProductInfo.Th09);
      Assert.IsNotNull(ProductInfo.Th095);
      Assert.IsNotNull(ProductInfo.Th10tr);
      Assert.IsNotNull(ProductInfo.Th10);
      Assert.IsNotNull(ProductInfo.Th10tr);
      Assert.IsNotNull(ProductInfo.Th10trWeb);
      Assert.IsNotNull(ProductInfo.Th11);
      Assert.IsNotNull(ProductInfo.Th11tr);
      Assert.IsNotNull(ProductInfo.Th11);
      Assert.IsNotNull(ProductInfo.Th12tr);
      Assert.IsNotNull(ProductInfo.Alcostg);

      Assert.AreEqual(17, ProductInfo.Th06.Tracks.Count,      "track count of Th06");
      Assert.AreEqual(20, ProductInfo.Th07.Tracks.Count,      "track count of Th07");
      Assert.AreEqual(7,  ProductInfo.Th08tr.Tracks.Count,    "track count of Th08tr");
      Assert.AreEqual(21, ProductInfo.Th08.Tracks.Count,      "track count of Th08");
      Assert.AreEqual(7,  ProductInfo.Th09tr.Tracks.Count,    "track count of Th09tr");
      Assert.AreEqual(19, ProductInfo.Th09.Tracks.Count,      "track count of Th09");
      Assert.AreEqual(6,  ProductInfo.Th095.Tracks.Count,     "track count of Th095");
      Assert.AreEqual(7,  ProductInfo.Th10tr.Tracks.Count,    "track count of Th10tr");
      Assert.AreEqual(8,  ProductInfo.Th10trWeb.Tracks.Count, "track count of Th10trWeb");
      Assert.AreEqual(18, ProductInfo.Th10.Tracks.Count,      "track count of Th10");
      Assert.AreEqual(8,  ProductInfo.Th11tr.Tracks.Count,    "track count of Th11tr");
      Assert.AreEqual(18, ProductInfo.Th11.Tracks.Count,      "track count of Th11");
      Assert.AreEqual(8,  ProductInfo.Th12tr.Tracks.Count,    "track count of Th12tr");
      Assert.AreEqual(18, ProductInfo.Th12.Tracks.Count,      "track count of Th12");

      foreach (var expected in new[] {
        new {Creator = ProductInfo.Th06.Creator,      Message = "creator of Th06"},
        new {Creator = ProductInfo.Th07.Creator,      Message = "creator of Th07"},
        new {Creator = ProductInfo.Th08.Creator,      Message = "creator of Th08"},
        new {Creator = ProductInfo.Th08tr.Creator,    Message = "creator of Th08tr"},
        new {Creator = ProductInfo.Th09.Creator,      Message = "creator of Th09"},
        new {Creator = ProductInfo.Th09tr.Creator,    Message = "creator of Th09tr"},
        new {Creator = ProductInfo.Th095.Creator,     Message = "creator of Th095"},
        new {Creator = ProductInfo.Th10.Creator,      Message = "creator of Th10"},
        new {Creator = ProductInfo.Th10tr.Creator,    Message = "creator of Th10tr"},
        new {Creator = ProductInfo.Th10trWeb.Creator, Message = "creator of Th10trWeb"},
        new {Creator = ProductInfo.Th11.Creator,      Message = "creator of Th11"},
        new {Creator = ProductInfo.Th11tr.Creator,    Message = "creator of Th11tr"},
        new {Creator = ProductInfo.Th12.Creator,      Message = "creator of Th12"},
        new {Creator = ProductInfo.Th12tr.Creator,    Message = "creator of Th12tr"},
      }) {
        Assert.AreEqual("上海アリス幻樂団", expected.Creator, expected.Message);
      }

      foreach (var product in ProductInfo.EmbeddedProducts) {
        Assert.IsTrue(product.IsEmbedded);
        Assert.IsNotNull(product.DefaultBgmSourcePath);
        Assert.IsNull(product.InstalledBgmSourcePath);
        Assert.AreEqual(product.DefaultBgmSourcePath, product.InstalledOrDefaultBgmSourcePath);

        Assert.IsTrue(Path.IsPathRooted(product.DefaultBgmSourcePath), "path must be rooted");

        if (Runtime.IsRunningOnWindows)
          Assert.IsTrue(product.DefaultBgmSourcePath.IndexOf(":\\") == 1, "path starts with X:\\");
        else
          Assert.IsTrue(product.DefaultBgmSourcePath.StartsWith("/"), "path starts with /");

        Assert.IsFalse(Path.GetFileName(product.DefaultBgmSourcePath).Contains(Path.DirectorySeparatorChar.ToString()), "must be file name");
      }

      foreach (var expected in new[] {
        new {Product = ProductInfo.Th06,      ShortName = "東方紅魔郷", AbbreviatedShortName = "紅魔郷", ShortestName = "紅", Prefix = "th06"},
        new {Product = ProductInfo.Th07,      ShortName = "東方妖々夢", AbbreviatedShortName = "妖々夢", ShortestName = "妖", Prefix = "th07"},
        new {Product = ProductInfo.Th08,      ShortName = "東方永夜抄", AbbreviatedShortName = "永夜抄", ShortestName = "永", Prefix = "th08"},
        new {Product = ProductInfo.Th08tr,    ShortName = "東方永夜抄", AbbreviatedShortName = "永夜抄", ShortestName = "永", Prefix = "th08tr"},
        new {Product = ProductInfo.Th09,      ShortName = "東方花映塚", AbbreviatedShortName = "花映塚", ShortestName = "花", Prefix = "th09"},
        new {Product = ProductInfo.Th09tr,    ShortName = "東方花映塚", AbbreviatedShortName = "花映塚", ShortestName = "花", Prefix = "th09tr"},
        new {Product = ProductInfo.Th095,     ShortName = "東方文花帖", AbbreviatedShortName = "文花帖", ShortestName = "文", Prefix = "th095"},
        new {Product = ProductInfo.Th10,      ShortName = "東方風神録", AbbreviatedShortName = "風神録", ShortestName = "風", Prefix = "th10"},
        new {Product = ProductInfo.Th10tr,    ShortName = "東方風神録", AbbreviatedShortName = "風神録", ShortestName = "風", Prefix = "th10tr"},
        new {Product = ProductInfo.Th10trWeb, ShortName = "東方風神録", AbbreviatedShortName = "風神録", ShortestName = "風", Prefix = "th10tr_web"},
        new {Product = ProductInfo.Th11,      ShortName = "東方地霊殿", AbbreviatedShortName = "地霊殿", ShortestName = "地", Prefix = "th11"},
        new {Product = ProductInfo.Th11tr,    ShortName = "東方地霊殿", AbbreviatedShortName = "地霊殿", ShortestName = "地", Prefix = "th11tr"},
        new {Product = ProductInfo.Th12,      ShortName = "東方星蓮船", AbbreviatedShortName = "星蓮船", ShortestName = "星", Prefix = "th12"},
        new {Product = ProductInfo.Th12tr,    ShortName = "東方星蓮船", AbbreviatedShortName = "星蓮船", ShortestName = "星", Prefix = "th12tr"},
      }) {
        Assert.AreEqual(expected.ShortName, expected.Product.ShortName, "ShortName of {0}", expected.Product.Title);
        Assert.AreEqual(expected.AbbreviatedShortName, expected.Product.AbbreviatedShortName, "AbbreviatedShortName of {0}", expected.Product.Title);
        Assert.AreEqual(expected.ShortestName, expected.Product.ShortestName, "ShortestName of {0}", expected.Product.Title);
        Assert.AreEqual(expected.Prefix, expected.Product.Prefix, "Prefix of {0}", expected.Product.Title);
      }

      foreach (var product in new[] {
        ProductInfo.Th06,
        ProductInfo.Th07,
        ProductInfo.Th08,
        ProductInfo.Th09,
        ProductInfo.Th095,
        ProductInfo.Th10,
        ProductInfo.Th11,
        ProductInfo.Th12,
        ProductInfo.Alcostg,
      }) {
        Assert.AreNotEqual(Guid.Empty, product.THxxBGMProductGuid);
      }
    }

    [Test]
    public void TestGetUnifiedTitle()
    {
      Assert.AreEqual("東方妖々夢　～ Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢　～ Perfect Cherry Blossom.", TitleUnificationOptions.NoUnify));
      Assert.AreEqual("東方妖々夢  ～ Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢  ～ Perfect Cherry Blossom.", TitleUnificationOptions.NoUnify));
      Assert.AreEqual("東方妖々夢  ～ Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢　～ Perfect Cherry Blossom.", TitleUnificationOptions.DoubleSpace));
      Assert.AreEqual("東方妖々夢　～ Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢  ～ Perfect Cherry Blossom.", TitleUnificationOptions.SingleIdeographicSpace));

      Assert.AreEqual("東方妖々夢　\u301c Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢　\u301c Perfect Cherry Blossom.", TitleUnificationOptions.NoUnify));
      Assert.AreEqual("東方妖々夢　\uff5e Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢　\uff5e Perfect Cherry Blossom.", TitleUnificationOptions.NoUnify));

      Assert.AreEqual("東方妖々夢　〜 Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢　～ Perfect Cherry Blossom.", TitleUnificationOptions.WaveDash));
      Assert.AreEqual("東方妖々夢　～ Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢　〜 Perfect Cherry Blossom.", TitleUnificationOptions.FullWidthTilde));

      Assert.AreEqual("東方妖々夢\u0020\u0020\u301c Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢\u3000\uff5e Perfect Cherry Blossom.", TitleUnificationOptions.DoubleSpace | TitleUnificationOptions.WaveDash));
      Assert.AreEqual("東方妖々夢\u3000\uff5e Perfect Cherry Blossom.", ProductInfo.GetUnifiedTitle("東方妖々夢\u0020\u0020\u301c Perfect Cherry Blossom.", TitleUnificationOptions.SingleIdeographicSpace | TitleUnificationOptions.FullWidthTilde));
    }

    [Test]
    public void TestGuessMissingInfoes()
    {
      var testfilename = "titles_th22tr.txt";

      try {
        if (File.Exists(testfilename))
          File.Delete(testfilename);

        File.WriteAllText(testfilename, @"@骨川アリス幻樂団\東方臑茶魔体験版\thbgm_tr.dat,東方臑茶魔　～ The Acquaintance Of My Daddy. 体験版
#=ProductInfo,Prefix,
99999999,99999999,99999999,
99999999,99999999,99999999,
99999999,99999999,99999999,スネ夫が激しい弾幕を展開する時に流れている曲
");

        var product = ProductInfo.LoadFrom(testfilename);

        Assert.IsFalse(product.IsEmbedded);
        Assert.AreEqual("骨川アリス幻樂団", product.Creator);
        Assert.AreEqual("th22tr", product.Prefix);
        Assert.AreEqual("東方臑茶魔", product.ShortName);
        Assert.AreEqual("臑茶魔", product.AbbreviatedShortName);
        Assert.AreEqual("臑", product.ShortestName);
      }
      finally {
        if (File.Exists(testfilename))
          File.Delete(testfilename);
      }
    }

    [Test]
    public void TestInstalledBgmSourcePathUpdated()
    {
      var testStreamFile = "test.thbgm.dat";
      var testTitleFile = "test.thbgm.txt";

      if (File.Exists(testStreamFile))
        File.Delete(testStreamFile);
      if (File.Exists(testTitleFile))
        File.Delete(testTitleFile);

      try {
        using (var stream = File.OpenWrite(testStreamFile)) {
          var writer = new BinaryWriter(stream);

          writer.Write(new byte[] {0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd});

          writer.Flush();
        }

        File.WriteAllText(testTitleFile, @"@thxx\thbgm.dat,testdata
#=ProductInfo,BgmSourceLength,8
0,4,4,
");

        var productInfo = ProductInfo.LoadFrom(testTitleFile);

        Assert.IsFalse(productInfo.IsEmbedded);

        Assert.IsNull(productInfo.InstalledBgmSourcePath);
        Assert.AreEqual(productInfo.DefaultBgmSourcePath, productInfo.InstalledOrDefaultBgmSourcePath);

        Assert.AreEqual(productInfo, ProductInfo.FindMatchedProduct(testStreamFile, new[] {productInfo}));

        Assert.AreEqual(testStreamFile, productInfo.InstalledBgmSourcePath);
        Assert.AreEqual(testStreamFile, productInfo.InstalledOrDefaultBgmSourcePath);
      }
      finally {
        if (File.Exists(testStreamFile))
          File.Delete(testStreamFile);
        if (File.Exists(testTitleFile))
          File.Delete(testTitleFile);
      }
    }

    [Test]
    public void TestSave()
    {
      var testTitleFile = "test.thbgm.txt";

      if (File.Exists(testTitleFile))
        File.Delete(testTitleFile);

      try {
        ProductInfo.Th08.Save(testTitleFile);

        Assert.IsTrue(File.Exists(testTitleFile));

        var saved = ProductInfo.LoadFrom(testTitleFile);
        var expected = ProductInfo.Th08;

        Assert.IsNotNull(saved);

        Assert.AreEqual(expected.Title, saved.Title);
        Assert.AreEqual(expected.DefaultBgmSourcePath, saved.DefaultBgmSourcePath);
        Assert.AreEqual(expected.Creator, saved.Creator);
        Assert.AreEqual(expected.Prefix, saved.Prefix);
        Assert.AreEqual(expected.ShortName, saved.ShortName);
        Assert.AreEqual(expected.ShortestName, saved.ShortestName);
        Assert.AreEqual(expected.ReleaseDate, saved.ReleaseDate);
        Assert.AreEqual(expected.Tracks.Count, saved.Tracks.Count);

        for (var i = 0; i < expected.Tracks.Count; i++) {
          Assert.AreEqual(expected.Tracks[i].IntroOffset, saved.Tracks[i].IntroOffset, "intro offset of track {0}", i);
          Assert.AreEqual(expected.Tracks[i].IntroLength, saved.Tracks[i].IntroLength, "intro length of track {0}", i);
          Assert.AreEqual(expected.Tracks[i].RepeatLength, saved.Tracks[i].RepeatLength, "repeat length of track {0}", i);
          Assert.AreEqual(expected.Tracks[i].Title, saved.Tracks[i].Title, "title of track {0}", i);
        }
      }
      finally {
        if (File.Exists(testTitleFile))
          File.Delete(testTitleFile);
      }
    }
  }
}