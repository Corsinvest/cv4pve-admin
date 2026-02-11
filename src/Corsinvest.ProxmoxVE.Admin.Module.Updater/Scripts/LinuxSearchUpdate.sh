/bin/sh -s <<'EOF'

detect_distro() {
    detected_distro="Unknown"
    os_type=$(uname -s)
    
    if [ "$os_type" = "Linux" ]; then
        if [ -f /etc/os-release ]; then
            detected_distro=$(grep '^NAME=' /etc/os-release 2>/dev/null | cut -d'"' -f2)
            [ -z "$detected_distro" ] && detected_distro=$(grep '^ID=' /etc/os-release 2>/dev/null | cut -d'=' -f2 | tr -d '"')
        elif [ -f /etc/lsb-release ]; then
            detected_distro=$(grep '^DISTRIB_ID=' /etc/lsb-release 2>/dev/null | cut -d'=' -f2)
        elif command -v lsb_release >/dev/null 2>&1; then
            detected_distro=$(lsb_release -si 2>/dev/null)
        elif [ -f /etc/redhat-release ]; then
            detected_distro=$(awk '{print $1}' /etc/redhat-release 2>/dev/null)
        elif [ -f /etc/fedora-release ]; then
            detected_distro="Fedora"
        elif [ -f /etc/gentoo-release ]; then
            detected_distro="Gentoo"
        elif [ -f /etc/slackware-version ]; then
            detected_distro="Slackware"
        elif [ -f /etc/void-release ]; then
            detected_distro="Void Linux"
        elif [ -d /etc/nixos ]; then
            detected_distro="NixOS"
        elif [ -f /etc/clearlinux-release ]; then
            detected_distro="Clear Linux"
        elif [ -f /etc/solus-release ]; then
            detected_distro="Solus"
        elif [ -f /etc/mageia-release ]; then
            detected_distro="Mageia"
        elif [ -f /etc/alpine-release ]; then
            detected_distro="Alpine Linux"
        fi
    elif [ "$os_type" = "FreeBSD" ]; then
        detected_distro="FreeBSD"
    elif [ "$os_type" = "OpenBSD" ]; then
        detected_distro="OpenBSD"
    elif [ "$os_type" = "NetBSD" ]; then
        detected_distro="NetBSD"
    elif [ "$os_type" = "DragonFly" ]; then
        detected_distro="DragonFly BSD"
    elif [ "$os_type" = "Darwin" ]; then
        detected_distro="macOS"
        if command -v sw_vers >/dev/null 2>&1; then
            macos_version=$(sw_vers -productVersion 2>/dev/null)
            [ -n "$macos_version" ] && detected_distro="macOS $macos_version"
        fi
    elif [ "$os_type" = "SunOS" ]; then
        detected_distro="Solaris"
    fi
    
    printf '%s' "$detected_distro"
}

determine_pkg_manager() {
    distro="$1"
    pkg_manager=""
    
    case "$distro" in
        *"Arch Linux"*|*"Manjaro"*|*"EndeavourOS"*|*"Artix"*|*"Garuda"*)
            command -v pacman >/dev/null 2>&1 && pkg_manager="pacman"
            ;;
        *"Debian"*|*"Ubuntu"*|*"Mint"*|*"Pop!_OS"*|*"Elementary"*|*"Raspbian"*|*"Kali"*|*"MX Linux"*|*"Zorin"*|*"Deepin"*|*"Parrot"*|*"antiX"*)
            command -v apt >/dev/null 2>&1 && pkg_manager="apt"
            ;;
        *"Fedora"*|*"CentOS"*|*"RHEL"*|*"Red Hat"*|*"Oracle Linux"*|*"Rocky"*|*"AlmaLinux"*)
            if command -v dnf >/dev/null 2>&1; then
                pkg_manager="dnf"
            elif command -v yum >/dev/null 2>&1; then
                pkg_manager="yum"
            fi
            ;;
        *"SUSE"*|*"openSUSE"*)
            command -v zypper >/dev/null 2>&1 && pkg_manager="zypper"
            ;;
        *"Alpine"*)
            command -v apk >/dev/null 2>&1 && pkg_manager="apk"
            ;;
        *"Gentoo"*)
            command -v emerge >/dev/null 2>&1 && pkg_manager="portage"
            ;;
        *"Void"*)
            command -v xbps-install >/dev/null 2>&1 && pkg_manager="xbps"
            ;;
        *"NixOS"*)
            command -v nix-env >/dev/null 2>&1 && pkg_manager="nix"
            ;;
        *"Clear Linux"*)
            command -v swupd >/dev/null 2>&1 && pkg_manager="swupd"
            ;;
        *"Solus"*)
            command -v eopkg >/dev/null 2>&1 && pkg_manager="eopkg"
            ;;
        *"FreeBSD"*|*"DragonFly BSD"*)
            command -v pkg >/dev/null 2>&1 && pkg_manager="pkg"
            ;;
        *"OpenBSD"*)
            command -v pkg_add >/dev/null 2>&1 && pkg_manager="pkg_add"
            ;;
        *"NetBSD"*)
            command -v pkgin >/dev/null 2>&1 && pkg_manager="pkgin"
            ;;
        *"macOS"*)
            if command -v softwareupdate >/dev/null 2>&1; then
                pkg_manager="softwareupdate"
            elif command -v brew >/dev/null 2>&1; then
                pkg_manager="brew"
            elif command -v port >/dev/null 2>&1; then
                pkg_manager="port"
            fi
            ;;
        *"Slackware"*)
            command -v slackpkg >/dev/null 2>&1 && pkg_manager="slackpkg"
            ;;
        *"Mageia"*)
            command -v urpmi >/dev/null 2>&1 && pkg_manager="urpmi"
            ;;
        *"Solaris"*)
            if command -v pkg >/dev/null 2>&1; then
                pkg_manager="pkg"
            elif command -v pkgutil >/dev/null 2>&1; then
                pkg_manager="pkgutil"
            fi
            ;;
        *)
            if command -v apt >/dev/null 2>&1; then
                pkg_manager="apt"
            elif command -v dnf >/dev/null 2>&1; then
                pkg_manager="dnf"
            elif command -v yum >/dev/null 2>&1; then
                pkg_manager="yum"
            elif command -v pacman >/dev/null 2>&1; then
                pkg_manager="pacman"
            elif command -v zypper >/dev/null 2>&1; then
                pkg_manager="zypper"
            elif command -v apk >/dev/null 2>&1; then
                pkg_manager="apk"
            fi
            ;;
    esac
    
    printf '%s' "$pkg_manager"
}

check_reboot_required() {
    distro="$1"
    require_reboot=false
    
    if [ -e /var/run/reboot-required ] || \
       [ -e /var/lib/update-notifier/reboot-required ] || \
       [ -e /run/reboot-required ] || \
       [ -f /usr/lib/reboot-required ] || \
       [ -f /var/run/need_reboot ]; then
        require_reboot=true
    elif [ -f /boot/kernel/kernel.old ] && [ "$distro" = "FreeBSD" ]; then
        require_reboot=true
    elif printf '%s' "$distro" | grep -qE "(Fedora|CentOS|RHEL)"; then
        if [ -f /usr/bin/needs-restarting ]; then
            /usr/bin/needs-restarting -r >/dev/null 2>&1 || require_reboot=true
        fi
    elif printf '%s' "$distro" | grep -q "macOS"; then
        if command -v softwareupdate >/dev/null 2>&1; then
            pending_update=$(softwareupdate -l 2>/dev/null | grep -i "restart")
            [ -n "$pending_update" ] && require_reboot=true
        fi
    fi
    
    printf '%s' "$require_reboot"
}

run_with_timeout() {
    command="$1"
    timeout_sec="$2"
    
    if command -v timeout >/dev/null 2>&1; then
        timeout "$timeout_sec" sh -c "$command" 2>/dev/null
    else
        eval "$command" 2>/dev/null
    fi
}

# Helper function to safely get numeric count from wc -l
get_line_count() {
    count=$(printf '%s' "$1" | wc -l | tr -d ' \t\n\r')
    # Ensure we have a valid number
    case "$count" in
        ''|*[!0-9]*) count=0 ;;
    esac
    printf '%s' "$count"
}

# Helper function to sanitize numeric values
sanitize_number() {
    number=$(printf '%s' "$1" | tr -d ' \t\n\r')
    case "$number" in
        ''|*[!0-9]*) number=0 ;;
    esac
    printf '%s' "$number"
}

check_updates() {
    pkg_manager="$1"
    update_normal=false
    update_security=false
    
    case "$pkg_manager" in
        "apt")
            if run_with_timeout "apt-get update -qq" 30; then
                upgradable_count=$(apt list --upgradable 2>/dev/null | grep -c "upgradable" 2>/dev/null || printf '0')
                upgradable_count=$(sanitize_number "$upgradable_count")
                if [ "$upgradable_count" -gt 0 ]; then
                    update_normal=true
                    
                    if apt list --upgradable 2>/dev/null | grep -qiE "(security|CVE|critical)"; then
                        update_security=true
                    fi
                fi
            fi
            ;;
        "dnf")
            run_with_timeout "dnf check-update -q" 30 >/dev/null 2>&1
            dnf_exit=$?
            if [ $dnf_exit -eq 100 ]; then
                update_normal=true
                if run_with_timeout "dnf updateinfo list security" 15 2>/dev/null | grep -qE "(Important|Critical|Moderate)"; then
                    update_security=true
                fi
            fi
            ;;
        "yum")
            run_with_timeout "yum check-update -q" 30 >/dev/null 2>&1
            yum_exit=$?
            if [ $yum_exit -eq 100 ]; then
                update_normal=true
                if run_with_timeout "yum updateinfo list security" 15 2>/dev/null | grep -qE "(Important|Critical|Moderate)"; then
                    update_security=true
                fi
            fi
            ;;
        "pacman")
            if run_with_timeout "pacman -Sy --noconfirm" 30; then
                updates_output=$(run_with_timeout "pacman -Qu" 15 2>/dev/null | grep -v "warning")
                updates_count=$(get_line_count "$updates_output")
                if [ "$updates_count" -gt 0 ]; then
                    update_normal=true
                    update_security=true
                fi
            fi
            ;;
        "apk")
            if run_with_timeout "apk update" 30; then
                upgrade_output=$(run_with_timeout "apk upgrade -s" 15 2>/dev/null)
                upgrade_count=$(printf '%s' "$upgrade_output" | grep -c "Upgrading" 2>/dev/null || printf '0')
                upgrade_count=$(sanitize_number "$upgrade_count")
                if [ "$upgrade_count" -gt 0 ]; then
                    update_normal=true
                    update_security=true
                fi
            fi
            ;;
        "zypper")
            if run_with_timeout "zypper refresh" 30 >/dev/null 2>&1; then
                if run_with_timeout "zypper list-updates --type package" 15 2>/dev/null | grep -qv "No updates found"; then
                    update_normal=true
                fi
                if run_with_timeout "zypper list-updates --type patch --category security" 15 2>/dev/null | grep -qv "No updates found"; then
                    update_security=true
                fi
            fi
            ;;
        "portage")
            if run_with_timeout "emerge --sync -q" 60 >/dev/null 2>&1; then
                if run_with_timeout "emerge -puDN --quiet world" 30 2>/dev/null | grep -qv "Total: 0"; then
                    update_normal=true
                fi
                if command -v glsa-check >/dev/null 2>&1 && run_with_timeout "glsa-check -t affected" 15 >/dev/null 2>&1; then
                    update_security=true
                fi
            fi
            ;;
        "xbps")
            if run_with_timeout "xbps-install -S" 30 >/dev/null 2>&1; then
                updates_output=$(run_with_timeout "xbps-install -un" 15 2>/dev/null | grep -v "to be upgraded")
                updates_count=$(get_line_count "$updates_output")
                [ "$updates_count" -gt 0 ] && update_normal=true
                update_security=false
            fi
            ;;
        "nix")
            if run_with_timeout "nix-channel --update" 60 >/dev/null 2>&1; then
                if run_with_timeout "nix-env -u --dry-run" 30 2>/dev/null | grep -qv "no updates"; then
                    update_normal=true
                fi
                update_security=false
            fi
            ;;
        "pkg")
            if run_with_timeout "pkg update" 30 >/dev/null 2>&1; then
                if run_with_timeout "pkg upgrade -n" 15 2>/dev/null | grep -qv "Your packages are up to date"; then
                    update_normal=true
                fi
                if run_with_timeout "pkg audit -F" 30 2>/dev/null | grep -qv "0 problem(s)"; then
                    update_security=true
                fi
            fi
            ;;
        "pkg_add")
            updates_output=$(run_with_timeout "pkg_add -snu" 30 2>/dev/null | grep -v "Can't find")
            updates_available=$(get_line_count "$updates_output")
            [ "$updates_available" -gt 0 ] && update_normal=true
            update_security=false
            ;;
        "pkgin")
            if run_with_timeout "pkgin update" 30 >/dev/null 2>&1; then
                if run_with_timeout "pkgin upgrade" 15 2>/dev/null | grep -qv "nothing to upgrade"; then
                    update_normal=true
                fi
                update_security=false
            fi
            ;;
        "brew")
            if run_with_timeout "brew update" 60 >/dev/null 2>&1; then
                outdated_output=$(run_with_timeout "brew outdated" 15 2>/dev/null)
                outdated_count=$(get_line_count "$outdated_output")
                [ "$outdated_count" -gt 0 ] && update_normal=true
                update_security=false
            fi
            ;;
        "port")
            if run_with_timeout "port selfupdate" 60 >/dev/null 2>&1; then
                outdated_output=$(run_with_timeout "port outdated" 30 2>/dev/null)
                outdated_count=$(get_line_count "$outdated_output")
                [ "$outdated_count" -gt 0 ] && update_normal=true
                update_security=false
            fi
            ;;
        "slackpkg")
            if run_with_timeout "slackpkg update" 60 >/dev/null 2>&1; then
                if run_with_timeout "slackpkg check-updates" 30 2>/dev/null | grep -q "Updated package(s) available"; then
                    update_normal=true
                fi
                update_security=false
            fi
            ;;
        "eopkg")
            if run_with_timeout "eopkg ur" 30 >/dev/null 2>&1; then
                if run_with_timeout "eopkg lu" 15 2>/dev/null | grep -qv "No packages to upgrade"; then
                    update_normal=true
                fi
                update_security=false
            fi
            ;;
        "urpmi")
            if run_with_timeout "urpmi.update -a" 60 >/dev/null 2>&1; then
                updates_output=$(run_with_timeout "urpmq --auto-select" 30 2>/dev/null)
                updates_count=$(get_line_count "$updates_output")
                [ "$updates_count" -gt 0 ] && update_normal=true
                update_security=false
            fi
            ;;
        "swupd")
            run_with_timeout "swupd check-update" 30 >/dev/null 2>&1
            swupd_exit=$?
            [ $swupd_exit -ne 0 ] && update_normal=true
            update_security=false
            ;;
        "softwareupdate")
            if command -v softwareupdate >/dev/null 2>&1; then
                updates=$(run_with_timeout "softwareupdate -l" 30 2>/dev/null)
                if [ -n "$updates" ] && ! printf '%s' "$updates" | grep -qi "No new software available"; then
                    update_normal=true
                    printf '%s' "$updates" | grep -qi "security" && update_security=true
                fi
            fi
            ;;
        *)
            update_normal=false
            update_security=false
            ;;
    esac
    
    printf '{ "update_normal_available": %s, "update_security_available": %s }' "$update_normal" "$update_security"
}

check_macos_updates() {
    update_normal=false
    update_security=false
    
    if command -v softwareupdate >/dev/null 2>&1; then
        updates=$(run_with_timeout "softwareupdate -l" 30 2>/dev/null)
        if [ -n "$updates" ] && ! printf '%s' "$updates" | grep -qi "No new software available"; then
            update_normal=true
            printf '%s' "$updates" | grep -qi "security" && update_security=true
        fi
    fi
    
    printf '{ "update_normal_available": %s, "update_security_available": %s }' "$update_normal" "$update_security"
}

main() {
    distro=$(detect_distro)
    pkg_manager=$(determine_pkg_manager "$distro")
    require_reboot=$(check_reboot_required "$distro")
    
    update_check=true
    update_normal_available=false
    update_security_available=false
    
    if [ -n "$pkg_manager" ] && [ "$pkg_manager" != "" ]; then
        updates=$(check_updates "$pkg_manager")

        if printf '%s' "$updates" | grep -q "update_normal_available"; then
            update_normal_available=$(printf '%s' "$updates" | sed -n 's/.*"update_normal_available": *\([^,}]*\).*/\1/p')
            update_security_available=$(printf '%s' "$updates" | sed -n 's/.*"update_security_available": *\([^,}]*\).*/\1/p')
        else
            update_check=false
        fi
    else
        update_check=false
    fi
    
    timestamp=$(date -u +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u)
    
    printf '{"distro": "%s", "package_manager": "%s", "update_check": %s, "update_normal_available": %s, "update_security_available": %s, "require_reboot": %s, "timestamp": "%s"}\n' \
        "$distro" "$pkg_manager" "$update_check" "$update_normal_available" "$update_security_available" "$require_reboot" "$timestamp"
}

main
EOF
