using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Models
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class MediaXmlTest : UmbracoIntegrationTest
    {
        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
        private IMediaService MediaService => GetRequiredService<IMediaService>();
        private IUserService UserService => GetRequiredService<IUserService>();
        private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();
        private IEntityXmlSerializer EntityXmlSerializer => GetRequiredService<IEntityXmlSerializer>();

        [Test]
        public void Can_Generate_Xml_Representation_Of_Media()
        {
            // Arrange
            var mediaType = MediaTypeBuilder.CreateImageMediaType("image2");
            MediaTypeService.Save(mediaType);

            var media = MediaBuilder.CreateMediaImage(mediaType, -1);
            media.WriterId = -1; // else it's zero and that's not a user and it breaks the tests
            MediaService.Save(media, Constants.Security.SuperUserId);

            // so we have to force-reset these values because the property editor has cleared them
            media.SetValue(Constants.Conventions.Media.Width, "200");
            media.SetValue(Constants.Conventions.Media.Height, "200");
            media.SetValue(Constants.Conventions.Media.Bytes, "100");
            media.SetValue(Constants.Conventions.Media.Extension, "png");

            var nodeName = media.ContentType.Alias.ToSafeAlias(ShortStringHelper);
            var urlName = media.GetUrlSegment(ShortStringHelper, new[] { new DefaultUrlSegmentProvider(ShortStringHelper) });

            // Act
            XElement element = media.ToXml(EntityXmlSerializer);

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Name.LocalName, Is.EqualTo(nodeName));
            Assert.AreEqual(media.Id.ToString(), (string)element.Attribute("id"));
            Assert.AreEqual(media.ParentId.ToString(), (string)element.Attribute("parentID"));
            Assert.AreEqual(media.Level.ToString(), (string)element.Attribute("level"));
            Assert.AreEqual(media.SortOrder.ToString(), (string)element.Attribute("sortOrder"));
            Assert.AreEqual(media.CreateDate.ToString("s"), (string)element.Attribute("createDate"));
            Assert.AreEqual(media.UpdateDate.ToString("s"), (string)element.Attribute("updateDate"));
            Assert.AreEqual(media.Name, (string)element.Attribute("nodeName"));
            Assert.AreEqual(urlName, (string)element.Attribute("urlName"));
            Assert.AreEqual(media.Path, (string)element.Attribute("path"));
            Assert.AreEqual("", (string)element.Attribute("isDoc"));
            Assert.AreEqual(media.ContentType.Id.ToString(), (string)element.Attribute("nodeType"));
            Assert.AreEqual(media.GetCreatorProfile(UserService).Name, (string)element.Attribute("writerName"));
            Assert.AreEqual(media.CreatorId.ToString(), (string)element.Attribute("writerID"));
            Assert.IsNull(element.Attribute("template"));

            Assert.AreEqual(media.Properties[Constants.Conventions.Media.File].GetValue().ToString(), element.Elements(Constants.Conventions.Media.File).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Width].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Width).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Height].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Height).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Bytes].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Bytes).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Extension].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Extension).Single().Value);
        }
    }
}
