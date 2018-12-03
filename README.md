View factor / area determination for solar flux computations.

Employs orthographic projections. Uses DirectX within the monogame framework for display.

Requires the following inputs
 1. Wavefront file (*.obj, ASCII) of the satellite. Each object within the file
     will be treated as a unique surface.
 2. Attitude evolution file. Create this file using the orekit software
     "AttitudePropagator".
 3. A settings file in *.xml format. Here, you need to specify the satellite
     name, input and output directories. The settings file must be located in
     the same folder as the executable.
     
 The current software requires a lot of refactoring and is part of a larger package
 to also a) compute thermal energy transfer and b) display the results.

The Fraunhofer-Gesellschaft zur Foerderung der angewandten Forschung e.V.,
Hansastrasse 27c, 80686 Munich, Germany (further: Fraunhofer) is the holder
of all proprietary rights on this computer program. You can only use this
computer program if you have closed a license agreement with Fraunhofer or
you get the right to use the computer program from someone who is authorized
to grant you that right. Any use of the computer program without a valid
license is prohibited and liable to prosecution.

The use of this software is only allowed under the terms and condition of the
General Public License version 2.0 (GPL 2.0).

Copyright©2018 Gesellschaft zur Foerderung der angewandten Forschung e.V. acting
on behalf of its Fraunhofer Institut für  Kurzzeitdynamik. All rights reserved.

Contact: max.gulde@emi.fraunhofer.de
