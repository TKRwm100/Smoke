using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using ObjectiveHarmonyPatch;
using SlimDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BveEx.Toukaitetudou.Smoke
{
    [Plugin(PluginType.VehiclePlugin)]
    public class PluginMain : AssemblyPluginBase
    {
        DisposableList<Model> GSmoke;
        DisposableList<Model> WSmoke;
        HarmonyPatch Patch;
        Vector3 GPos;
        Vector3 WPos;
        public PluginMain(PluginBuilder builder) : base(builder)
        {
            Patch=HarmonyPatch.Patch(nameof(Patch), BveHacker.BveTypes.GetClassInfoOf<StructureDrawer>().GetSourceMethodOf(nameof(StructureDrawer.Draw)).Source, PatchType.Postfix);
            Patch.Invoked+=Patch_Invoked;
            string basePath = Path.GetDirectoryName(Location);
            Config config;
            string filePath = Path.Combine(basePath, $"{nameof(Smoke)}.Config.xml");
            using (StreamReader sr = new StreamReader(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Config));
                try
                {
                    config = (Config)xs.Deserialize(sr);
                }
                catch (Exception exp)
                {
                    _ = exp;
                    config = null;
                }
            }
            GSmoke=config.Chimney.Structure.Select(x => Model.FromXFile(Path.Combine(basePath, x.Path))).ToDisposableList();
            WSmoke=            config.Drain.Structure.Select(x => Model.FromXFile(Path.Combine(basePath, x.Path))).ToDisposableList();
            BveHacker.MainFormSource.KeyDown+=MainFormSource_KeyDown;

            GPos=new Vector3((float)config.Chimney.X, (float)config.Chimney.Y, (float)config.Chimney.Z);
            WPos=new Vector3((float)config.Drain.X, (float)config.Drain.Y, (float)config.Drain.Z);

            GMaxElapse=config.Chimney.LifeSpan.GetTimeSpan();
            WMaxElapse=config.Drain.LifeSpan.GetTimeSpan();
            GSpan=config.Chimney.GenerateSpan.GetTimeSpan();
            WSpan=config.Drain.GenerateSpan.GetTimeSpan();
            DolenContinue=config.Drain.ContinueTime.GetTimeSpan();

        }


        bool DolenSW=false;
        TimeSpan DolenElapsed;
        TimeSpan DolenContinue;
        bool Dolen;
        private void MainFormSource_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode==System.Windows.Forms.Keys.PageUp)
            {
                DolenSW=!DolenSW;
                DolenElapsed=TimeSpan.Zero;
            }
        }

        private PatchInvokationResult Patch_Invoked(object sender, PatchInvokedEventArgs e)
        {
            Direct3DProvider direct3DProvider = Direct3DProvider.FromSource(e.Args[0]);
            Matrix viewMatrix = (Matrix)e.Args[1];

            Matrix billboardingMatrix = Matrix.Invert(viewMatrix);
            billboardingMatrix.M41 = 0;
            billboardingMatrix.M42 = 0;
            billboardingMatrix.M43 = 0;

            foreach (Sprite sprite in Sprites)
            {
                Matrix matrix = billboardingMatrix* BveHacker.Scenario.Map.GetTrackMatrix(sprite.Structure, sprite.Structure.Location, BveHacker.Scenario.VehicleLocation.BlockIndex*25)*Matrix.Translation(sprite.GetTranslation())*

                    viewMatrix;


                direct3DProvider.Device.SetTransform(SlimDX.Direct3D9.TransformState.World,
                matrix
                );
                sprite.Structure.Model.SetAlpha((int)(255-255*sprite.TotalElapsed.TotalSeconds/sprite.MaxElapsed.TotalSeconds));
                sprite.Structure.Model.Draw(direct3DProvider, false);
                sprite.Structure.Model.Draw(direct3DProvider, true);

            }
            return new PatchInvokationResult(SkipModes.Continue);
        }

        public override void Dispose()
        {
            Patch?.Dispose();
            GSmoke?.Dispose();
            WSmoke?.Dispose();
            BveHacker.MainFormSource.KeyDown-=MainFormSource_KeyDown;
        }
        TimeSpan GMaxElapse;
        TimeSpan WMaxElapse;
        List<Sprite> Sprites = new List<Sprite>();
        TimeSpan GTotalElapsed;
        TimeSpan WTotalElapsed;
        TimeSpan GSpan;
        TimeSpan WSpan;
        Random R = new Random();
        public override void Tick(TimeSpan elapsed)
        {

            GTotalElapsed+=elapsed;
            DolenElapsed+=elapsed;
            Dolen=DolenSW&&DolenElapsed<=DolenContinue;
            if (GTotalElapsed >= GSpan)
            {
                GTotalElapsed-=GSpan;
                Sprites.Add(new Sprite(new BveTypes.ClassWrappers.Structure(BveHacker.Scenario.VehicleLocation.Location, "0", GPos.X, GPos.Y, GPos.Z, 0, 0, 0, TiltOptions.Default, 0, GSmoke[R.Next(0,GSmoke.Count-1)]), GMaxElapse, (TimeSpan x) => new Vector3(0, (float)x.TotalSeconds*1, 0)));
            }
            if (Dolen)
            {
                WTotalElapsed+=elapsed;
                if (WTotalElapsed >= WSpan)
                {
                    WTotalElapsed-=WSpan;
                    Sprites.Add(new Sprite(new BveTypes.ClassWrappers.Structure(BveHacker.Scenario.VehicleLocation.Location, "0", WPos.X, WPos.Y, WPos.Z, 0, 0, 0, TiltOptions.Default, 0, WSmoke[R.Next(0, WSmoke.Count-1)]), WMaxElapse,
                        (TimeSpan x) =>
                        {
                            return new Vector3((float)(-0.5*x.TotalSeconds), 0, 0);
                        }
                        ));
                }
            }

            for (int i = 0; i < Sprites.Count; i++)
            {
                Sprite sprite = Sprites[i];
                sprite?.Tick(elapsed);
                if (sprite?.IsDestruct??true)
                {
                    Sprites.RemoveAt(i);
                }
            }
            return;
        }
    }
}