( function ()
{
    const raiz = document.documentElement;
    const boton = document.getElementById( 'theme-toggle' );
    const icono = boton ? boton.querySelector( 'i' ) : null;

    function pintarIcono ( tema )
    {
        if ( icono ) icono.className = tema === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
    }

    pintarIcono( raiz.getAttribute( 'data-bs-theme' ) );

    if ( boton )
    {
        boton.addEventListener( 'click', function ()
        {
            const siguiente = raiz.getAttribute( 'data-bs-theme' ) === 'dark' ? 'light' : 'dark';
            raiz.setAttribute( 'data-bs-theme', siguiente );
            localStorage.setItem( 'tm-theme', siguiente );
            pintarIcono( siguiente );
        } );
    }

    // Los avisos de éxito se cierran solos a los 5 segundos.
    document.querySelectorAll( '.alert-auto' ).forEach( function ( aviso )
    {
        setTimeout( function () { bootstrap.Alert.getOrCreateInstance( aviso ).close(); }, 5000 );
    } );
} )();