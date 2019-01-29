/* View factor / area determination for solar flux computations.
 *
 * Entry.
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
 * Copyright©2019 Gesellschaft zur Foerderung der angewandten Forschung e.V. acting
 * on behalf of its Fraunhofer Institut für  Kurzzeitdynamik. All rights reserved.
 * 
 * Contact: max.gulde@emi.fraunhofer.de
 * 
 */

using System;

namespace Thermal
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Main())
                game.Run();
        }
    }
#endif
}
