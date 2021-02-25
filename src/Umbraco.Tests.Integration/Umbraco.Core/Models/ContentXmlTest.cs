using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
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
    public class ContentXmlTest : UmbracoIntegrationTest
    {
        private IFileService FileService => GetRequiredService<IFileService>();
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
        private IContentService ContentService => GetRequiredService<IContentService>();
        private IEntityXmlSerializer EntityXmlSerializer => GetRequiredService<IEntityXmlSerializer>();
        private IUserService UserService => GetRequiredService<IUserService>();

        [Test]
        public void Can_Generate_Xml_Representation_Of_Content()
        {
            // Arrange
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template); // Else nullreference exception.

            var contentType = ContentTypeBuilder.CreateTextPageContentType("test1", "Test1", template.Id);
            FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            ContentTypeService.Save(contentType);

            var content = ContentBuilder.CreateTextpageContent(contentType, "Root Home", -1);
            ContentService.Save(content, Constants.Security.SuperUserId);

            var nodeName = content.ContentType.Alias.ToSafeAlias(ShortStringHelper);
            var urlName = content.GetUrlSegment(ShortStringHelper, new[]{new DefaultUrlSegmentProvider(ShortStringHelper) });

            // Act
            XElement element = content.ToXml(EntityXmlSerializer);

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Name.LocalName, Is.EqualTo(nodeName));
            Assert.AreEqual(content.Id.ToString(), (string)element.Attribute("id"));
            Assert.AreEqual(content.ParentId.ToString(), (string)element.Attribute("parentID"));
            Assert.AreEqual(content.Level.ToString(), (string)element.Attribute("level"));
            Assert.AreEqual(content.CreatorId.ToString(), (string)element.Attribute("creatorID"));
            Assert.AreEqual(content.SortOrder.ToString(), (string)element.Attribute("sortOrder"));
            Assert.AreEqual(content.CreateDate.ToString("s"), (string)element.Attribute("createDate"));
            Assert.AreEqual(content.UpdateDate.ToString("s"), (string)element.Attribute("updateDate"));
            Assert.AreEqual(content.Name, (string)element.Attribute("nodeName"));
            Assert.AreEqual(urlName, (string)element.Attribute("urlName"));
            Assert.AreEqual(content.Path, (string)element.Attribute("path"));
            Assert.AreEqual("", (string)element.Attribute("isDoc"));
            Assert.AreEqual(content.ContentType.Id.ToString(), (string)element.Attribute("nodeType"));
            Assert.AreEqual(content.GetCreatorProfile(UserService).Name, (string)element.Attribute("creatorName"));
            Assert.AreEqual(content.GetWriterProfile(UserService).Name, (string)element.Attribute("writerName"));
            Assert.AreEqual(content.WriterId.ToString(), (string)element.Attribute("writerID"));
            Assert.AreEqual(content.TemplateId.ToString(), (string)element.Attribute("template"));

            Assert.AreEqual(content.Properties["title"].GetValue().ToString(), element.Elements("title").Single().Value);
            Assert.AreEqual(content.Properties["bodyText"].GetValue().ToString(), element.Elements("bodyText").Single().Value);
            Assert.AreEqual(content.Properties["keywords"].GetValue().ToString(), element.Elements("keywords").Single().Value);
            Assert.AreEqual(content.Properties["description"].GetValue().ToString(), element.Elements("description").Single().Value);
        }
    }
}
