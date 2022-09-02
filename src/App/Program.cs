using PixelWindowSystem;

var raycasterAppManager = new RaycasterAppManager();

var window = new PixelWindow(1280, 768, 2, "Jallen's Raycaster", raycasterAppManager,
    fixedTimestep: 20, framerateLimit: 300);

window.Run();
