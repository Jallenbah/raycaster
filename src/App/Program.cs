using PixelWindowSystem;

var raycasterAppManager = new RaycasterAppManager();

var window = new PixelWindow(1024, 576, 16, "Jallen's Raycaster", raycasterAppManager,
    fixedTimestep: 20, framerateLimit: 300);

window.Run();
