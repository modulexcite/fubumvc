using System;
using System.IO;
using System.Reflection;
using Bottles;
using Bottles.Assemblies;
using Bottles.Diagnostics;
using FubuMVC.Core.Packaging;
using FubuTestingSupport;
using NUnit.Framework;
using FubuCore;
using System.Linq;
using Rhino.Mocks;

namespace FubuMVC.Tests.Packaging
{
    [TestFixture]
    public class PackageManifestReaderIntegratedTester
    {
        private string packageFolder;
        private PackageManifestReader reader;
        private string theApplicationDirectory = "../../".ToFullPath();
        private LinkedFolderPackageLoader linkedFolderReader;

        [SetUp]
        public void SetUp()
        {
            packageFolder = FileSystem.Combine("../../../TestPackage1").ToFullPath();

            var fileSystem = new FileSystem();
            var manifest = new PackageManifest(){
                Name = "pak1"
            };

            manifest.AddAssembly("TestPackage1");

            fileSystem.PersistToFile(manifest, packageFolder, PackageManifest.FILE);

            linkedFolderReader = new LinkedFolderPackageLoader(theApplicationDirectory, f => f);

            reader = new PackageManifestReader(fileSystem, folder => folder);
        }



        [TearDown]
        public void TearDown()
        {
            new FileSystem().DeleteFile(FileSystem.Combine(theApplicationDirectory, PackageManifest.FILE));
        }



        [Test]
		[Platform(Exclude="Mono")]
        public void load_a_package_info_from_a_manifest_file_when_given_the_folder()
        {
            // the reader is rooted at the folder location of the main app
            var package = reader.LoadFromFolder("../../../TestPackage1".ToFullPath());

            var assemblyLoader = new AssemblyLoader(new PackagingDiagnostics());
            assemblyLoader.AssemblyFileLoader = file => Assembly.Load(File.ReadAllBytes(file));
            assemblyLoader.LoadAssembliesFromPackage(package);

            var loadedAssemblies = assemblyLoader.Assemblies.ToArray();
            loadedAssemblies.ShouldHaveCount(1);
            loadedAssemblies[0].GetName().Name.ShouldEqual("TestPackage1");
        }

        [Test]
        public void load_a_package_registers_web_content_folder()
        {
            var packageDirectory = "../../../TestPackage1".ToFullPath();
            var package = reader.LoadFromFolder(packageDirectory);
            var directoryContinuation = MockRepository.GenerateMock<Action<string>>();

            package.ForFolder(BottleFiles.WebContentFolder, directoryContinuation);
        
            directoryContinuation.AssertWasCalled(x => x.Invoke(packageDirectory));
        }

		[Test]
		[Platform(Exclude="Mono")]
		public void load_packages_by_assembly()
		{
			var includes = new PackageManifest();
            
            new FileSystem().PersistToFile(includes, theApplicationDirectory, PackageManifest.FILE);

		    var links = new LinkManifest();
            links.AddLink("../TestPackage1");

            new FileSystem().PersistToFile(links, theApplicationDirectory, LinkManifest.FILE);

			var assemblyLoader = new AssemblyLoader(new PackagingDiagnostics());
            assemblyLoader.AssemblyFileLoader = file => Assembly.Load(File.ReadAllBytes(file));

			var package = linkedFolderReader.Load(new PackageLog()).Single();
			assemblyLoader.LoadAssembliesFromPackage(package);

			assemblyLoader
				.Assemblies
				.Single()
				.GetName()
				.Name
				.ShouldEqual("TestPackage1");
		}
    }
}