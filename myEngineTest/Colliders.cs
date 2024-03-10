namespace MyEngine {
    public interface ICollider {
        float X {get;}
    }
    public record BoxCollider(float Width) : ICollider {
        public float X => Width;
    }
}