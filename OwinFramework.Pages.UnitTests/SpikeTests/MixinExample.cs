using NUnit.Framework;

namespace OwinFramework.Pages.UnitTests.SpikeTests
{
    public interface IResizable
    {
        float Width { get; set; }
        float Height { get; set; }
        bool MaintainAspectRatio { get; set; }
    }

    public class ResizableMixin : IResizable
    {
        private float _width;
        private float _height;

        public float Width
        {
            get { return _width; }
            set
            {
                if (MaintainAspectRatio)
                    _height = (_height / _width) * value;

                _width = value;
            }
        }

        public float Height
        {
            get { return _height; }
            set
            {
                if (MaintainAspectRatio)
                    _width = (_width / _height) * value;

                _height = value;
            }
        }

        public bool MaintainAspectRatio { get; set; }
    }

    public class Ellipse : IResizable
    {
        public float X { get; set; }
        public float Y { get; set; }

        #region IResizable mixin

        private readonly ResizableMixin _resizable = new ResizableMixin();

        float IResizable.Width
        {
            get { return _resizable.Width; }
            set { _resizable.Width = value; }
        }

        float IResizable.Height
        {
            get { return _resizable.Height; }
            set { _resizable.Height = value; }
        }

        bool IResizable.MaintainAspectRatio
        {
            get { return _resizable.MaintainAspectRatio; }
            set { _resizable.MaintainAspectRatio = value; }
        }

        #endregion
    }

    public class Rectangle : IResizable
    {
        public float X { get; set; }
        public float Y { get; set; }

        #region IResizable mixin

        private readonly ResizableMixin _resizable = new ResizableMixin();

        float IResizable.Width
        {
            get { return _resizable.Width; }
            set { _resizable.Width = value; }
        }

        float IResizable.Height
        {
            get { return _resizable.Height; }
            set { _resizable.Height = value; }
        }

        bool IResizable.MaintainAspectRatio
        {
            get { return _resizable.MaintainAspectRatio; }
            set { _resizable.MaintainAspectRatio = value; }
        }

        #endregion
    }

    [TestFixture]
    public class MixinTests
    {
        [Test]
        public void Should_maintain_aspect_ratio_on_width_change()
        {
            Should_maintain_aspect_ratio_on_width_change(new ResizableMixin());
            Should_maintain_aspect_ratio_on_width_change(new Ellipse());
            Should_maintain_aspect_ratio_on_width_change(new Rectangle());
        }

        private void Should_maintain_aspect_ratio_on_width_change(IResizable resizable)
        {
            resizable.Width = 10;
            resizable.Height = 20;

            Assert.AreEqual(10, resizable.Width, 0.01);
            Assert.AreEqual(20, resizable.Height, 0.01);

            resizable.MaintainAspectRatio = true;
            resizable.Width = 20;

            Assert.AreEqual(40, resizable.Height, 0.01);
        }

        [Test]
        public void Should_maintain_aspect_ratio_on_height_change()
        {
            Should_maintain_aspect_ratio_on_height_change(new ResizableMixin());
            Should_maintain_aspect_ratio_on_height_change(new Ellipse());
            Should_maintain_aspect_ratio_on_height_change(new Rectangle());
        }

        private void Should_maintain_aspect_ratio_on_height_change(IResizable resizable)
        {
            resizable.Width = 10;
            resizable.Height = 20;

            Assert.AreEqual(10, resizable.Width, 0.01);
            Assert.AreEqual(20, resizable.Height, 0.01);

            resizable.MaintainAspectRatio = true;
            resizable.Height = 40;

            Assert.AreEqual(20, resizable.Width, 0.01);
        }

        [Test]
        public void Should_not_maintain_aspect_ratio_by_default()
        {
            Should_not_maintain_aspect_ratio_by_default(new ResizableMixin());
            Should_not_maintain_aspect_ratio_by_default(new Ellipse());
            Should_not_maintain_aspect_ratio_by_default(new Rectangle());
        }

        private void Should_not_maintain_aspect_ratio_by_default(IResizable resizable)
        {
            resizable.Width = 10;
            resizable.Height = 20;

            Assert.AreEqual(10, resizable.Width, 0.01);
            Assert.AreEqual(20, resizable.Height, 0.01);

            resizable.Height = 40;

            Assert.AreEqual(10, resizable.Width, 0.01);
            Assert.AreEqual(40, resizable.Height, 0.01);

            resizable.Width = 20;

            Assert.AreEqual(20, resizable.Width, 0.01);
            Assert.AreEqual(40, resizable.Height, 0.01);
        }
    }

}