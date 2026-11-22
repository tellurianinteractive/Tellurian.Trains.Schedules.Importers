# Tellurian.Trains.Schedules.Model.Geometry

Pure 2D geometry for the drawings of the Tellurian Trains suite: points, vectors, angles, poses and transforms; line and arc segments and paths built from them; projection onto paths, distances, joins, polygon offsets, areas, spatial indexing and shape comparison.

- Units are millimetres and degrees; angles counter-clockwise from the X axis, Y up.
- Tolerances are in one place, `Tolerance`.
- No domain knowledge and no dependencies. The layout model (`Tellurian.Trains.Schedules.Model.Layouts`) and Module Draw build on it.

Moved here from Module Draw (`Tellurian.Trains.Drawing.Geometry`) in version 4.0.5; only the namespace changed.
