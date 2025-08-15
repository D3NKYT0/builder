@echo off
chcp 65001 >nul
echo ========================================
echo    UpdateBuilder - Clean Script
echo ========================================
echo.

:: Navegar para o diretório do projeto
cd /d "%~dp0.."

echo Limpando arquivos de build e temporários...
echo.

:: Remover pasta bin
if exist "bin" (
    echo Removendo pasta 'bin'...
    rmdir /s /q "bin" >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✓ Pasta 'bin' removida com sucesso.
    ) else (
        echo ✗ Erro ao remover pasta 'bin'.
    )
) else (
    echo - Pasta 'bin' não encontrada.
)

:: Remover pasta obj
if exist "obj" (
    echo Removendo pasta 'obj'...
    rmdir /s /q "obj" >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✓ Pasta 'obj' removida com sucesso.
    ) else (
        echo ✗ Erro ao remover pasta 'obj'.
    )
) else (
    echo - Pasta 'obj' não encontrada.
)

:: Remover arquivos temporários do Visual Studio
if exist "*.user" (
    echo Removendo arquivos .user...
    del /q "*.user" >nul 2>&1
    echo ✓ Arquivos .user removidos.
)

if exist "*.suo" (
    echo Removendo arquivos .suo...
    del /q "*.suo" >nul 2>&1
    echo ✓ Arquivos .suo removidos.
)

if exist "*.cache" (
    echo Removendo arquivos .cache...
    del /q "*.cache" >nul 2>&1
    echo ✓ Arquivos .cache removidos.
)

:: Remover arquivos de log
if exist "*.log" (
    echo Removendo arquivos .log...
    del /q "*.log" >nul 2>&1
    echo ✓ Arquivos .log removidos.
)

echo.
echo ========================================
echo    LIMPEZA CONCLUÍDA!
echo ========================================
echo.
echo O projeto foi limpo com sucesso.
echo Execute 'build.bat' ou 'build-debug.bat' para recompilar.
echo.
pause
