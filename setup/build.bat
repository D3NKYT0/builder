@echo off
chcp 65001 >nul
echo ========================================
echo    UpdateBuilder - Build Script
echo ========================================
echo.

:: Navegar para o diretório do projeto
cd /d "%~dp0.."

:: Verificar se o .NET 6.0 SDK está instalado
echo [1/5] Verificando .NET 6.0 SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: .NET SDK não encontrado!
    echo Por favor, instale o .NET 6.0 SDK
    echo Download: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo .NET SDK encontrado: %DOTNET_VERSION%
echo.

:: Limpar builds anteriores
echo [2/5] Limpando builds anteriores...
if exist "bin" (
    rmdir /s /q "bin" >nul 2>&1
    echo Builds anteriores removidos.
)
if exist "obj" (
    rmdir /s /q "obj" >nul 2>&1
    echo Objetos temporários removidos.
)
echo.

:: Restaurar dependências
echo [3/5] Restaurando dependências NuGet...
dotnet restore
if %errorlevel% neq 0 (
    echo ERRO: Falha ao restaurar dependências!
    pause
    exit /b 1
)
echo Dependências restauradas com sucesso.
echo.

:: Compilar o projeto
echo [4/5] Compilando o projeto...
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo ERRO: Falha na compilação!
    pause
    exit /b 1
)
echo Compilação concluída com sucesso!
echo.

:: Verificar se o executável foi criado
echo [5/5] Verificando arquivo executável...
if exist "bin\Release\net6.0-windows\L2JCore Builder.exe" (
    echo Executável criado: bin\Release\net6.0-windows\L2JCore Builder.exe
    echo.
    echo ========================================
    echo    BUILD CONCLUÍDO COM SUCESSO!
    echo ========================================
    echo.
    
    :: Perguntar se deseja executar
    set /p RUN_APP="Deseja executar o aplicativo agora? (S/N): "
    if /i "%RUN_APP%"=="S" (
        echo Executando UpdateBuilder...
        start "" "bin\Release\net6.0-windows\L2JCore Builder.exe"
    )
) else (
    echo ERRO: Executável não encontrado!
    echo Verifique se a compilação foi bem-sucedida.
    pause
    exit /b 1
)

echo.
echo Pressione qualquer tecla para sair...
pause >nul
