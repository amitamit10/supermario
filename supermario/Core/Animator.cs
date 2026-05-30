using System.Drawing;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  Animator — מחזור פריימים פשוט / simple frame cycler
    /// --------------------------------------------------------------------
    ///  מחזיק רשימת תמונות ומחליף ביניהן כל כמה "טיקים". משמש את האויבים
    ///  לאנימציית ההליכה: קוראים ל-Tick() כל פריים, וכשהוא מחזיר true
    ///  מציבים את Current ב-PictureBox.Image.
    ///
    ///  Holds a list of images and switches between them every few ticks.
    ///  Used by enemies for the walk cycle: call Tick() each frame, and when
    ///  it returns true assign Current to the PictureBox.Image.
    /// ════════════════════════════════════════════════════════════════════
    internal sealed class Animator
    {
        private readonly Image[] _frames;
        private readonly int _ticksPerFrame;
        private int _tick;
        private int _index;

        public Animator(Image[] frames, int ticksPerFrame)
        {
            _frames = frames;
            _ticksPerFrame = ticksPerFrame < 1 ? 1 : ticksPerFrame;
        }

        // התמונה הנוכחית / the current frame image
        public Image Current => (_frames != null && _frames.Length > 0) ? _frames[_index] : null;

        // מקדם את הטיימר; מחזיר true כשהפריים התחלף / advances; true when the frame changed
        public bool Tick()
        {
            if (_frames == null || _frames.Length < 2) return false;
            _tick++;
            if (_tick < _ticksPerFrame) return false;
            _tick = 0;
            _index = (_index + 1) % _frames.Length;
            return true;
        }
    }
}
