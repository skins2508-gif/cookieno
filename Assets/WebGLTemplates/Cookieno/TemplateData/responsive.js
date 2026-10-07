// The board and its anchored controls are authored at 1920 x 1080.
// Keep that coordinate system at every browser size instead of stretching it.
(function () {
  var container = document.getElementById('unity-container');
  var canvas = document.getElementById('unity-canvas');
  var footer = document.getElementById('unity-footer');

  function resizeGame() {
    var fullscreen = document.fullscreenElement === container;
    var footerHeight = fullscreen ? 0 : footer.offsetHeight;
    var availableHeight = Math.max(0, window.innerHeight - footerHeight);
    // Multiples of 16 x 9 also keep the backing canvas ratio exact at DPR 1.
    var unit = Math.max(1, Math.floor(Math.min(window.innerWidth / 16, availableHeight / 9)));
    var width = unit * 16;
    var height = unit * 9;
    canvas.style.width = width + 'px';
    canvas.style.height = height + 'px';
    container.style.width = width + 'px';
  }

  window.addEventListener('resize', resizeGame);
  document.addEventListener('fullscreenchange', resizeGame);
  resizeGame();

  window.enterGameFullscreen = function () {
    if (container.requestFullscreen) {
      container.requestFullscreen().catch(function (error) {
        console.warn('Fullscreen request failed:', error);
      });
    }
  };
})();
