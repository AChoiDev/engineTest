using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;

public interface IRenderable {
    void Render();
}

public record MeshInstanceRender(Mesh Mesh)
: IRenderDesc
{
    public void Render() {}
}

public record ParticleRender(string GraphName)
: IRenderDesc
{
    public void Render() {}
}

public interface IRenderDesc {
    void Render();
}


namespace MyEngine {
    public static class Tick {
        
    }
    public interface IPhysical : ITransformable {
        Physicality Phys {get;}
    }

    public record Physicality(float MassKg, ICollider Collider, float Gravity = 9.8f);

    public interface ITransformable {
        Transform Tra {get; set;}
    }

    public record struct Transform(Vector3 Pos)
    {
        public void Move(Vector3 Delta) => Pos += Delta;
        public void SetPos(Vector3 NewPos) => Pos = NewPos;
    }

    public interface IExecutable {
        Task Execute();
    }

    public class Apple
    : ITransformable {
        public Transform Tra {get; set;}
    }

    

    public class Player : IPhysical, IExecutable
    {
        public Transform Tra {get; set;}
        public Physicality Phys => new(
            Collider: new BoxCollider(5f),
            MassKg: 1f
        );

        public async Task Execute() {
            // setup here
            while (true) {
                // logic here
                await World.Yield();
            }
        }
    }

    public record Entity {

    }

    public class ProtoRLBox
    : IRenderable, IExecutable
    {
        public Vector3 Position {get; set;}
        public float Height {get; set;}
        public Color Color {get; set;}

        public async Task Execute()
        {
            var startingPos = Position;
    
            var thingy = new Random();

            int iterations = thingy.Next(0, 10000);
            var isTemp = thingy.Next(0, 2) == 1;
            while (true) {
                Position = startingPos + (Vector3.UnitY * MathF.Sin(iterations * 0.05f));
                iterations += 1;
                if (isTemp && thingy.Next(0, 10) == 0) {
                    break;
                }
                await World.Yield();
            }
        }

        public void Render() {
            Raylib.DrawCube(Position, 2.0f, Height, 2.0f, Color);
            Raylib.DrawCubeWires(Position, 2.0f, Height, 2.0f, Color.Maroon);
        }
    }
}

