/* View factor / area determination for solar flux computations.
 *
 * Manages Keyboard and Mouse.
 * 
 * The Fraunhofer-Gesellschaft zur Foerderung der angewandten Forschung e.V.,
 * Hansastrasse 27c, 80686 Munich, Germany (further: Fraunhofer) is the holder
 * of all proprietary rights on this computer program. You can only use this
 * computer program if you have closed a license agreement with Fraunhofer or
 * you get the right to use the computer program from someone who is authorized
 * to grant you that right. Any use of the computer program without a valid
 * license is prohibited and liable to prosecution.
 * 
 * The use of this software is only allowed under the terms and condition of the
 * General Public License version 2.0 (GPL 2.0).
 * 
 * Copyright©2018 Gesellschaft zur Foerderung der angewandten Forschung e.V. acting
 * on behalf of its Fraunhofer Institut für  Kurzzeitdynamik. All rights reserved.
 * 
 * Contact: max.gulde@emi.fraunhofer.de
 * 
 */

#region using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

#endregion

namespace Thermal
{
    public class Input
    {
        #region fields

        Stopwatch sw_KeyDelay;
        Stopwatch sw_SimDelay;

        int MouseWheelPosOld;

        #endregion

        public Input()
        {
            sw_KeyDelay = new Stopwatch();
            sw_KeyDelay.Start();
            sw_SimDelay = new Stopwatch();
            sw_SimDelay.Start();

            MouseWheelPosOld = Mouse.GetState().ScrollWheelValue;
        }

        #region general

        public bool Exit
        {
            get
            {
                return Keyboard.GetState().IsKeyDown(Keys.Escape);
            }
        }

        public bool ClickL
        {
            get
            {
                return Mouse.GetState().LeftButton == ButtonState.Pressed;
            }
        }

        public Point MousePosition
        {
            get
            {
                return Mouse.GetState().Position;
            }
        }

        public bool LoadSettings
        {
            get
            {
                return Keyboard.GetState().IsKeyDown(Keys.F9) && KeyDelay;
            }
        }

        public bool SaveSettings
        {
            get
            {
                return Keyboard.GetState().IsKeyDown(Keys.F5) && KeyDelay;
            }
        }

        bool KeyDelay
        {
            get
            {
                if (sw_KeyDelay.ElapsedMilliseconds >= Settings.KeyDelay)
                {
                    sw_KeyDelay.Restart();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SimDelay
        {
            get
            {
                if (sw_SimDelay.ElapsedMilliseconds >= Settings.SimDelay)
                {      
                    sw_SimDelay.Restart();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region FastView_Solar

        public bool Pause
        {
            get
            {
                return Keyboard.GetState().IsKeyDown(Keys.Space) && KeyDelay;
            }
        }

        public bool Start
        {
            get
            {
                return Keyboard.GetState().IsKeyDown(Keys.Enter);
            }
        }

        public bool Next
        {
            get
            {
                return Right && KeyDelay;
            }
        }

        public bool Previous
        {
            get
            {
                return Left && KeyDelay;
            }
        }

        public bool ToggleWrite
        {
            get
            {
                return Activate && KeyDelay;
            }
        }

        #endregion

        //#region thermodisplay

        //public bool ToggleColoring
        //{
        //    get
        //    {
        //        return Keyboard.GetState().IsKeyDown(Keys.T) && KeyDelay;
        //    }
        //}

        //public bool ToggleWireframe
        //{
        //    get
        //    {
        //        return Keyboard.GetState().IsKeyDown(Keys.W) && KeyDelay;
        //    }
        //}

        //public int Zoom
        //{
        //    get
        //    {
        //        int MouseWheelPos = Mouse.GetState().ScrollWheelValue;
        //        int diff = MouseWheelPos - MouseWheelPosOld;
        //        MouseWheelPosOld = Mouse.GetState().ScrollWheelValue;
        //        return diff;
        //    }
        //}

        //public bool AltZoom
        //{
        //    get
        //    {
        //        return Shift;
        //    }
        //}

        //public bool Forward
        //{
        //    get
        //    {
        //        return Right && !Shift && !Control;
        //    }
        //}

        //public bool FForward
        //{
        //    get
        //    {
        //        return Right && Shift && !Control;
        //    }
        //}

        //public bool FFForward
        //{
        //    get
        //    {
        //        return Right && Shift && Control;
        //    }
        //}

        //public bool SForward
        //{
        //    get
        //    {
        //        return Right && !Shift && Control && KeyDelay;
        //    }
        //}

        //public bool Backward
        //{
        //    get
        //    {
        //        return Left && !Shift && !Control;
        //    }
        //}

        //public bool FBackward
        //{
        //    get
        //    {
        //        return Left && Shift && !Control;
        //    }
        //}

        //public bool FFBackward
        //{
        //    get
        //    {
        //        return Left && Shift && Control;
        //    }
        //}

        //public bool SBackward
        //{
        //    get
        //    {
        //        return Left && !Shift && Control && KeyDelay;
        //    }
        //}

        //public bool ChangeTemperatureType
        //{
        //    get
        //    {
        //        return Keyboard.GetState().IsKeyDown(Keys.Tab) && KeyDelay;
        //    }
        //}

        //public bool ToggleInset
        //{
        //    get
        //    {
        //        return Keyboard.GetState().IsKeyDown(Keys.Space) && KeyDelay;
        //    }
        //}

        //public bool ToggleContinuousDisplay
        //{
        //    get
        //    {
        //        return Keyboard.GetState().IsKeyDown(Keys.Enter) && KeyDelay;
        //    }
        //}

        //#endregion

        #region internal

        bool Shift
        {
            get
            {
                return (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift));
            }
        }

        bool Control
        {
            get
            {
                return (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl));
            }
        }

        bool Right
        {
            get
            {
                return (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.NumPad6));
            }
        }

        bool Left
        {
            get
            {
                return (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.NumPad4));
            }
        }

        bool Activate
        {
            get
            {
                return Keyboard.GetState().IsKeyDown(Keys.F12);
            }
        }

        #endregion

    }
}
