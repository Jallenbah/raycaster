using PixelWindowSystem;

var raycasterAppManager = new RaycasterAppManager();

var window = new PixelWindow(1024, 576, 2, "Jallen's Raycaster", raycasterAppManager,
    fixedTimestep: 20, framerateLimit: 300);

window.Run();
